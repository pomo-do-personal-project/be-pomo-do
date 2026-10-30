using System.Text.RegularExpressions;
using be_pomo_do.Application.Common;
using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Pomodoro;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.Services;

// Application service implementing Pomodoro focus sessions, fast-forward testing, and streak calculation.
public class PomodoroService : IPomodoroService
{
    private readonly IPomodoroRepository _pomodoroRepository;
    private readonly ITaskRepository _taskRepository;

    // Initializes a new instance of PomodoroService.
    public PomodoroService(IPomodoroRepository pomodoroRepository, ITaskRepository taskRepository)
    {
        _pomodoroRepository = pomodoroRepository ?? throw new ArgumentNullException(nameof(pomodoroRepository));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task<FocusSessionDto> StartSessionAsync(Guid userId, StartSessionRequest request)
    {
        if (request.TaskId.HasValue)
        {
            var task = await _taskRepository.GetTaskByIdAsync(request.TaskId.Value, userId, includeDeleted: false);
            if (task == null)
            {
                throw new DomainException("Công việc gắn với phiên không tồn tại hoặc đã bị xóa.");
            }

            // Tự động chuyển Task sang InProgress nếu đang ở trạng thái Todo
            if (task.Status == TaskStatus.Todo)
            {
                task.Status = TaskStatus.InProgress;
                task.UpdatedAt = DateTime.UtcNow;
            }
        }

        var session = new FocusSession
        {
            UserId = userId,
            TaskId = request.TaskId,
            Mode = request.Mode,
            DurationMinutes = request.DurationMinutes,
            ActualSeconds = 0,
            OvertimeSeconds = 0,
            IsCompleted = false,
            StartedAt = DateTime.UtcNow
        };

        await _pomodoroRepository.AddSessionAsync(session);
        await _pomodoroRepository.SaveChangesAsync();

        return MapToDto(session);
    }

    public async Task<FocusSessionDto> CompleteSessionAsync(Guid sessionId, Guid userId, CompleteSessionRequest request)
    {
        var session = await _pomodoroRepository.GetSessionByIdAsync(sessionId, userId);
        if (session == null)
        {
            throw new DomainException("Phiên tập trung không tồn tại hoặc bạn không có quyền truy cập.");
        }

        if (session.CompletedAt.HasValue && session.IsCompleted)
        {
            throw new DomainException("Phiên tập trung này đã được xác nhận hoàn thành trước đó.");
        }

        var now = DateTime.UtcNow;
        var plannedSeconds = session.DurationMinutes * 60;
        var elapsedRealSeconds = (int)Math.Max(0, (now - session.StartedAt).TotalSeconds);
        var maxAllowedSeconds = elapsedRealSeconds + 60;

        // Chống gian lận thời gian (Anti-Time-Tampering):
        // ActualSeconds tối đa không được vượt quá thời gian trôi qua thực tế cộng dung sai mạng (60 giây)
        int actualSeconds;
        if (request.ActualSeconds > 0)
        {
            actualSeconds = Math.Min(request.ActualSeconds, maxAllowedSeconds);
        }
        else
        {
            actualSeconds = Math.Min(plannedSeconds, maxAllowedSeconds);
        }

        // Overtime tính toán nghiêm ngặt dựa trên actualSeconds thực tế đã kiểm chứng (hỗ trợ cả gửi riêng hoặc gộp)
        var maxSeparateOvertime = Math.Max(0, maxAllowedSeconds - actualSeconds);
        var maxIncludedOvertime = Math.Max(0, actualSeconds - plannedSeconds);
        var maxOvertime = Math.Max(maxSeparateOvertime, maxIncludedOvertime);
        var overtimeSeconds = request.OvertimeSeconds > 0
            ? Math.Min(request.OvertimeSeconds, maxOvertime)
            : maxIncludedOvertime;

        session.ActualSeconds = actualSeconds;
        session.OvertimeSeconds = overtimeSeconds;
        session.IsCompleted = true;
        session.CompletedAt = now;

        // Giới hạn độ dài note tối đa 2000 ký tự và chuẩn hóa điểm số AI nếu có
        var note = request.Note?.Trim();
        if (!string.IsNullOrEmpty(note))
        {
            note = Regex.Replace(note, @"\[AI\s+(\d+)%\]", m =>
            {
                if (int.TryParse(m.Groups[1].Value, out var score))
                {
                    return $"[AI {Math.Clamp(score, 0, 100)}%]";
                }
                return m.Value;
            });
            session.Note = note.Length > 2000 ? note[..2000] : note;
        }
        else
        {
            session.Note = null;
        }

        // Tự động chuyển Task sang Completed nếu người dùng xác nhận đã hoàn thành
        if (request.IsTaskCompleted == true && session.TaskId.HasValue)
        {
            var task = await _taskRepository.GetTaskByIdAsync(session.TaskId.Value, userId, includeDeleted: false);
            if (task != null)
            {
                task.Status = TaskStatus.Completed;
                task.UpdatedAt = now;
            }
        }

        // Cập nhật DailyStreak cho người dùng
        await UpdateStreakAsync(userId);

        await _pomodoroRepository.SaveChangesAsync();
        return MapToDto(session);
    }

    public async Task<FocusSessionDto> CancelSessionAsync(Guid sessionId, Guid userId, CancelSessionRequest request)
    {
        var session = await _pomodoroRepository.GetSessionByIdAsync(sessionId, userId);
        if (session == null)
        {
            throw new DomainException("Phiên tập trung không tồn tại hoặc bạn không có quyền truy cập.");
        }

        // Chặn gian lận: Không thể hủy một phiên đã hoàn tất thành công
        if (session.CompletedAt.HasValue && session.IsCompleted)
        {
            throw new DomainException("Không thể hủy bỏ một phiên tập trung đã hoàn tất.");
        }

        var now = DateTime.UtcNow;
        var elapsedRealSeconds = (int)Math.Max(0, (now - session.StartedAt).TotalSeconds);
        var actualSeconds = request.ActualSeconds > 0
            ? Math.Min(request.ActualSeconds, elapsedRealSeconds + 60)
            : elapsedRealSeconds;

        session.ActualSeconds = actualSeconds;
        session.IsCompleted = false;
        session.CompletedAt = now;

        await _pomodoroRepository.SaveChangesAsync();
        return MapToDto(session);
    }

    public async Task<List<FocusSessionDto>> GetHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        // Giới hạn truy vấn lịch sử tối đa 365 ngày để bảo vệ hiệu năng hệ thống
        var effectiveFrom = fromUtc ?? DateTime.UtcNow.AddDays(-90);
        var effectiveTo = toUtc ?? DateTime.UtcNow;

        if (effectiveTo < effectiveFrom)
        {
            throw new DomainException("Khoảng thời gian không hợp lệ: ngày kết thúc phải sau ngày bắt đầu.");
        }

        if ((effectiveTo - effectiveFrom).TotalDays > 365)
        {
            effectiveFrom = effectiveTo.AddDays(-365);
        }

        var sessions = await _pomodoroRepository.GetSessionHistoryAsync(userId, effectiveFrom, effectiveTo);
        return sessions.Select(MapToDto).ToList();
    }

    public async Task<PomodoroSummaryDto> GetSummaryAsync(Guid userId)
    {
        var localToday = TimeZoneHelper.GetLocalDate(DateTime.UtcNow);
        var todayLocalStart = localToday.ToDateTime(TimeOnly.MinValue);
        var todayUtcStart = TimeZoneInfo.ConvertTimeToUtc(todayLocalStart, TimeZoneHelper.GetLocalTimeZone());

        var todaySessions = await _pomodoroRepository.GetSessionHistoryAsync(userId, todayUtcStart, null);

        var completedToday = todaySessions.Where(s => s.IsCompleted).ToList();
        var pomosCount = completedToday.Count(s => s.Mode == FocusSessionMode.Pomo);
        var totalActualSeconds = completedToday.Sum(s => s.ActualSeconds);
        var totalOvertimeSeconds = completedToday.Sum(s => s.OvertimeSeconds);

        var streak = await _pomodoroRepository.GetDailyStreakByUserIdAsync(userId);

        int currentStreak = 0;
        int longestStreak = 0;
        DateOnly? lastActive = null;

        if (streak != null)
        {
            longestStreak = streak.LongestStreak;
            lastActive = streak.LastActiveDate;

            var today = localToday;
            // Nếu ngày active gần nhất là hôm nay hoặc hôm qua thì streak còn hiệu lực
            if (streak.LastActiveDate == today || streak.LastActiveDate == today.AddDays(-1))
            {
                currentStreak = streak.CurrentStreak;
            }
            else
            {
                currentStreak = 0; // Đã bị đứt streak
            }
        }

        return new PomodoroSummaryDto
        {
            TotalPomosToday = pomosCount,
            TotalFocusMinutesToday = (int)Math.Round((double)totalActualSeconds / 60),
            TotalOvertimeMinutesToday = (int)Math.Max(0, Math.Round((double)totalOvertimeSeconds / 60)),
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            LastActiveDate = lastActive
        };
    }

    // Thuật toán cập nhật DailyStreak
    private async Task UpdateStreakAsync(Guid userId)
    {
        var streak = await _pomodoroRepository.GetDailyStreakByUserIdAsync(userId);
        var today = TimeZoneHelper.GetLocalDate(DateTime.UtcNow);

        if (streak == null)
        {
            streak = new DailyStreak
            {
                UserId = userId,
                CurrentStreak = 1,
                LongestStreak = 1,
                LastActiveDate = today
            };
            await _pomodoroRepository.AddDailyStreakAsync(streak);
            return;
        }

        if (streak.LastActiveDate == today)
        {
            // Đã ghi nhận hôm nay, không tăng thêm
            return;
        }

        if (streak.LastActiveDate == today.AddDays(-1))
        {
            // Liên tiếp ngày hôm qua -> Tăng streak
            streak.CurrentStreak += 1;
            streak.LongestStreak = Math.Max(streak.LongestStreak, streak.CurrentStreak);
        }
        else
        {
            // Đứt chuỗi -> Reset về 1
            streak.CurrentStreak = 1;
            streak.LongestStreak = Math.Max(streak.LongestStreak, 1);
        }

        streak.LastActiveDate = today;
    }

    private static FocusSessionDto MapToDto(FocusSession session)
    {
        return new FocusSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            TaskId = session.TaskId,
            TaskTitle = session.Task?.Title,
            Mode = session.Mode,
            DurationMinutes = session.DurationMinutes,
            ActualSeconds = session.ActualSeconds,
            OvertimeSeconds = session.OvertimeSeconds,
            IsCompleted = session.IsCompleted,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            Note = session.Note
        };
    }
}
