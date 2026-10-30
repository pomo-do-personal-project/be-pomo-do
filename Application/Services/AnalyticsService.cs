using System.Text.RegularExpressions;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Analytics;
using Microsoft.Extensions.Logging;

namespace be_pomo_do.Application.Services;

// Dịch vụ tổng hợp ma trận bản đồ nhiệt năng suất 7x24 và phân tích nhịp sinh học AI
public class AnalyticsService : IAnalyticsService
{
    private static readonly string[] DayNames = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
    private readonly IPomodoroRepository _pomodoroRepository;
    private readonly ILogger<AnalyticsService> _logger;

    // Khởi tạo AnalyticsService với Traditional Constructor
    public AnalyticsService(
        IPomodoroRepository pomodoroRepository,
        ILogger<AnalyticsService> logger)
    {
        _pomodoroRepository = pomodoroRepository ?? throw new ArgumentNullException(nameof(pomodoroRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Lấy dữ liệu ma trận 168 ô nhiệt và insight năng suất
    public async Task<FocusHeatmapResponse> GetFocusHeatmapAsync(Guid userId, int rangeDays = 30, CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var fromUtc = nowUtc.AddDays(-Math.Max(1, Math.Min(90, rangeDays)));
        var sessions = await _pomodoroRepository.GetSessionHistoryAsync(userId, fromUtc, nowUtc);

        var timeZone = be_pomo_do.Application.Common.TimeZoneHelper.GetLocalTimeZone();
        var cellDict = new Dictionary<(int Day, int Hour), HeatmapCellDto>();

        // Khởi tạo đủ 168 ô cho 7 ngày x 24 giờ
        for (var day = 0; day < 7; day++)
        {
            for (var hour = 0; hour < 24; hour++)
            {
                cellDict[(day, hour)] = new HeatmapCellDto
                {
                    DayOfWeek = day,
                    DayName = DayNames[day],
                    Hour = hour,
                    HourLabel = $"{hour:D2}:00 - {hour:D2}:59",
                    TotalMinutes = 0,
                    CompletedCount = 0,
                    CancelledCount = 0,
                    AverageAiScore = 0,
                    Level = 0
                };
            }
        }

        var totalScoreSum = 0;
        var totalScoreCount = 0;
        var totalDeepFlowSecs = 0;
        var totalActualSecs = 0;
        var cellScoreMap = new Dictionary<(int Day, int Hour), (int Sum, int Count)>();

        foreach (var session in sessions)
        {
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(session.StartedAt, timeZone);
            var startDayIndex = ((int)localStart.DayOfWeek + 6) % 7; // Monday = 0, Sunday = 6
            var startHour = localStart.Hour;

            if (cellDict.TryGetValue((startDayIndex, startHour), out var startCell))
            {
                if (session.IsCompleted)
                {
                    startCell.CompletedCount++;
                }
                else
                {
                    startCell.CancelledCount++;
                }
            }

            totalActualSecs += session.ActualSeconds;

            // Phân bổ chính xác số phút theo từng khung giờ nếu phiên vắt qua ranh giới giờ (ví dụ 08:45 -> 09:45)
            var currTime = localStart;
            var remainingSecs = session.ActualSeconds;
            while (remainingSecs > 0)
            {
                var currDay = ((int)currTime.DayOfWeek + 6) % 7;
                var currHour = currTime.Hour;
                var nextHourBoundary = currTime.Date.AddHours(currTime.Hour + 1);
                var secsInThisHour = Math.Min(remainingSecs, (int)Math.Max(1, (nextHourBoundary - currTime).TotalSeconds));

                if (cellDict.TryGetValue((currDay, currHour), out var targetCell))
                {
                    targetCell.TotalMinutes += secsInThisHour / 60;
                }

                remainingSecs -= secsInThisHour;
                currTime = nextHourBoundary;
            }

            var aiScore = ExtractAiScoreFromNote(session.Note);
            if (aiScore.HasValue)
            {
                totalScoreSum += aiScore.Value;
                totalScoreCount++;
                var (currSum, currCount) = cellScoreMap.TryGetValue((startDayIndex, startHour), out var current) ? current : (0, 0);
                cellScoreMap[(startDayIndex, startHour)] = (currSum + aiScore.Value, currCount + 1);
                totalDeepFlowSecs += (int)(session.ActualSeconds * (Math.Min(100, Math.Max(0, aiScore.Value)) / 100.0));
            }
        }

        // Tính điểm trung bình thật cho từng ô
        foreach (var ((day, hour), (sum, count)) in cellScoreMap)
        {
            if (cellDict.TryGetValue((day, hour), out var cell) && count > 0)
            {
                cell.AverageAiScore = sum / count;
            }
        }

        // Tính toán cấp độ nhiệt (Level 0 - 4) cho từng ô
        foreach (var cell in cellDict.Values)
        {
            if (cell.TotalMinutes == 0) cell.Level = 0;
            else if (cell.TotalMinutes < 25) cell.Level = 1;
            else if (cell.TotalMinutes < 50) cell.Level = 2;
            else if (cell.TotalMinutes < 90) cell.Level = 3;
            else cell.Level = 4;

            if (cell.AverageAiScore >= 90 && cell.TotalMinutes >= 45)
            {
                cell.Level = 4;
            }
        }

        var cellsList = cellDict.Values.OrderBy(c => c.DayOfWeek).ThenBy(c => c.Hour).ToList();
        var insight = GenerateProductivityInsight(sessions, cellDict, totalScoreSum, totalScoreCount, totalDeepFlowSecs, totalActualSecs);

        return new FocusHeatmapResponse
        {
            Cells = cellsList,
            Insight = insight,
            FromUtc = fromUtc,
            ToUtc = nowUtc,
            TotalFocusMinutes = totalActualSecs / 60,
            TotalCompletedSessions = sessions.Count(s => s.IsCompleted)
        };
    }

    // Bóc tách điểm số AI thật từ trường Note của phiên (trả về null nếu không có camera AI)
    private static int? ExtractAiScoreFromNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note)) return null;

        var match = Regex.Match(note, @"\[AI\s+(\d+)%\]");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var score))
        {
            return Math.Clamp(score, 0, 100);
        }

        var jsonMatch = Regex.Match(note, @"""score""\s*:\s*(\d+)");
        if (jsonMatch.Success && int.TryParse(jsonMatch.Groups[1].Value, out var jsonScore))
        {
            return Math.Clamp(jsonScore, 0, 100);
        }

        return null;
    }

    // Thuật toán nhận diện nhịp sinh học và khung giờ vàng
    private static AiProductivityInsightDto GenerateProductivityInsight(
        List<Core.Entities.FocusSession> sessions,
        Dictionary<(int Day, int Hour), HeatmapCellDto> cells,
        int totalScoreSum,
        int totalScoreCount,
        int totalDeepFlowSecs,
        int totalActualSecs)
    {
        if (sessions.Count < 3)
        {
            return new AiProductivityInsightDto
            {
                GoldenHourWindow = "Chưa đủ dữ liệu",
                GoldenHourScore = 0,
                VulnerableWindow = "Chưa đủ dữ liệu",
                VulnerableReason = "Cần tối thiểu 3 phiên tập trung để phân tích xu hướng",
                CircadianProfile = "Đang thu thập dữ liệu",
                TotalSessionsAnalyzed = sessions.Count,
                DeepFlowPercentage = 0,
                ActionableTip = "Hãy hoàn thành tối thiểu 3 phiên Pomodoro để AI bắt đầu phân tích nhịp sinh học và khung giờ vàng của bạn!"
            };
        }

        var hourScores = new int[24];
        var hourCancels = new int[24];
        var morningMins = 0;
        var afternoonMins = 0;
        var nightMins = 0;

        foreach (var cell in cells.Values)
        {
            var h = cell.Hour;
            var hourScoreBase = cell.AverageAiScore > 0 ? cell.AverageAiScore : 70;
            hourScores[h] += (cell.CompletedCount * hourScoreBase) + (cell.TotalMinutes * 2);
            hourCancels[h] += cell.CancelledCount * 50;

            if (h >= 5 && h < 12) morningMins += cell.TotalMinutes;
            else if (h >= 12 && h < 18) afternoonMins += cell.TotalMinutes;
            else nightMins += cell.TotalMinutes;
        }

        // Tìm khung giờ vàng (cửa sổ 2 giờ liên tiếp có tổng điểm cao nhất)
        var bestStartHour = 8;
        var bestScore = -1;
        for (var h = 6; h <= 21; h++)
        {
            var windowScore = hourScores[h] + hourScores[(h + 1) % 24] - (hourCancels[h] + hourCancels[(h + 1) % 24]);
            if (windowScore > bestScore)
            {
                bestScore = windowScore;
                bestStartHour = h;
            }
        }

        // Tìm khung giờ dễ bị xao nhãng hoặc hủy phiên nhất
        var worstHour = 14;
        var worstCancel = -1;
        for (var h = 0; h < 24; h++)
        {
            if (hourCancels[h] > worstCancel)
            {
                worstCancel = hourCancels[h];
                worstHour = h;
            }
        }

        // Xác định hồ sơ nhịp sinh học
        string profile;
        string tip;
        if (morningMins >= afternoonMins && morningMins >= nightMins)
        {
            profile = "Morning Achiever";
            tip = $"Năng lượng tập trung của bạn bùng nổ nhất vào buổi sáng ({bestStartHour:D2}:00 - {bestStartHour + 2:D2}:00). Hãy xếp các công việc Khẩn cấp & Quan trọng trước bữa trưa!";
        }
        else if (nightMins >= morningMins && nightMins >= afternoonMins)
        {
            profile = "Night Owl";
            tip = $"Bạn đạt phong độ tập trung tốt nhất vào khung giờ tối ({bestStartHour:D2}:00 - {bestStartHour + 2:D2}:00). Hãy giữ không gian tĩnh lặng và đừng quên ngủ đủ giấc.";
        }
        else
        {
            profile = "Steady Sprinter";
            tip = $"Bạn duy trì sự tập trung khá đồng đều suốt cả ngày. Hãy chú ý nghỉ ngắn giữa các phiên để tránh mỏi mắt và suy giảm năng suất.";
        }

        var avgScore = totalScoreCount > 0
            ? totalScoreSum / totalScoreCount
            : (int)(sessions.Count(s => s.IsCompleted) * 100.0 / Math.Max(1, sessions.Count));
        var deepFlowPct = totalActualSecs > 0 ? (int)((totalDeepFlowSecs / (double)totalActualSecs) * 100) : 0;

        return new AiProductivityInsightDto
        {
            GoldenHourWindow = $"{bestStartHour:D2}:00 - {bestStartHour + 2:D2}:00",
            GoldenHourScore = Math.Clamp(avgScore, 0, 100),
            VulnerableWindow = $"{worstHour:D2}:00 - {worstHour + 1:D2}:30",
            VulnerableReason = "Khung giờ có tỷ lệ hủy phiên hoặc gián đoạn cao",
            CircadianProfile = profile,
            TotalSessionsAnalyzed = sessions.Count,
            DeepFlowPercentage = Math.Clamp(deepFlowPct, 0, 100),
            ActionableTip = tip
        };
    }
}
