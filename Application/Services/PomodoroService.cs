using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Pomodoro;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.Services;

/// <summary>Application service implementing Pomodoro focus sessions, fast-forward testing, and streak calculation.</summary>
public class PomodoroService : IPomodoroService
{
    private readonly IPomodoroRepository _pomodoroRepository;
    private readonly ITaskRepository _taskRepository;

    /// <summary>Initializes a new instance of <see cref="PomodoroService"/>.</summary>
    public PomodoroService(IPomodoroRepository pomodoroRepository, ITaskRepository taskRepository)
    {
        _pomodoroRepository = pomodoroRepository ?? throw new ArgumentNullException(nameof(pomodoroRepository));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    /// <inheritdoc/>
    public async Task<FocusSessionDto> StartSessionAsync(Guid userId, StartSessionRequest request)
    {
        if (request.TaskId.HasValue)
        {
            var task = await _taskRepository.GetTaskByIdAsync(request.TaskId.Value, userId, includeDeleted: false);
            if (task == null)
            {
                throw new DomainException("Công việc gắn với phiên không tồn tại hoặc đã bị xóa.");
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

    /// <inheritdoc/>
    public async Task<FocusSessionDto> CompleteSessionAsync(Guid sessionId, Guid userId, CompleteSessionRequest request)
    {
        var session = await _pomodoroRepository.GetSessionByIdAsync(sessionId, userId);
        if (session == null)
        {
            throw new DomainException("Phiên tập trung không tồn tại hoặc bạn không có quyền truy cập.");
        }

        var now = DateTime.UtcNow;
        var plannedSeconds = session.DurationMinutes * 60;

        // Nếu client truyền actualSeconds > 0 thì dùng client, nếu không truyền hoặc gửi 0 thì tự tính dựa theo thời gian thực tế đã trôi qua
        var actualSeconds = request.ActualSeconds > 0
            ? request.ActualSeconds
            : (int)Math.Max(0, (now - session.StartedAt).TotalSeconds);

        // Tính overtime: Nếu client gửi overtimeSeconds > 0 thì dùng, nếu không thì tự tính (actualSeconds - plannedSeconds)
        var overtimeSeconds = request.OvertimeSeconds > 0
            ? request.OvertimeSeconds
            : Math.Max(0, actualSeconds - plannedSeconds);

        session.ActualSeconds = actualSeconds;
        session.OvertimeSeconds = overtimeSeconds;
        session.IsCompleted = true;
        session.CompletedAt = now;

        session.Note = request.Note?.Trim();

        // Cập nhật DailyStreak cho người dùng
        await UpdateStreakAsync(userId);

        await _pomodoroRepository.SaveChangesAsync();
        return MapToDto(session);
    }

    /// <inheritdoc/>
    public async Task<FocusSessionDto> CancelSessionAsync(Guid sessionId, Guid userId, CancelSessionRequest request)
    {
        var session = await _pomodoroRepository.GetSessionByIdAsync(sessionId, userId);
        if (session == null)
        {
            throw new DomainException("Phiên tập trung không tồn tại hoặc bạn không có quyền truy cập.");
        }

        session.ActualSeconds = request.ActualSeconds;
        session.IsCompleted = false;
        session.CompletedAt = DateTime.UtcNow;

        await _pomodoroRepository.SaveChangesAsync();
        return MapToDto(session);
    }

    /// <inheritdoc/>
    public async Task<List<FocusSessionDto>> GetHistoryAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var sessions = await _pomodoroRepository.GetSessionHistoryAsync(userId, fromUtc, toUtc);
        return sessions.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<PomodoroSummaryDto> GetSummaryAsync(Guid userId)
    {
        var todayUtcStart = DateTime.UtcNow.Date;
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

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
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
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

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
