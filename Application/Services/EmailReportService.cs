using be_pomo_do.Application.Common.Interfaces;
using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Core.ValueObjects;
using be_pomo_do.Infrastructure.Templates;
using Microsoft.Extensions.Logging;
using TaskStatus = be_pomo_do.Core.ValueObjects.TaskStatus;

namespace be_pomo_do.Application.Services;

// Dịch vụ tổng hợp số liệu và điều phối gửi email báo cáo năng suất qua Redis Streams
public class EmailReportService : IEmailReportService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPomodoroRepository _pomodoroRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IEmailService _emailService;
    private readonly IEmailReportQueue _reportQueue;
    private readonly ILogger<EmailReportService> _logger;

    // Khởi tạo EmailReportService với đầy đủ dependencies qua Traditional Constructor
    public EmailReportService(
        ISettingsRepository settingsRepository,
        IUserRepository userRepository,
        IPomodoroRepository pomodoroRepository,
        ITaskRepository taskRepository,
        IEmailService emailService,
        IEmailReportQueue reportQueue,
        ILogger<EmailReportService> logger)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _pomodoroRepository = pomodoroRepository ?? throw new ArgumentNullException(nameof(pomodoroRepository));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _reportQueue = reportQueue ?? throw new ArgumentNullException(nameof(reportQueue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Gửi báo cáo năng suất tuần cho người dùng
    public async Task SendWeeklyReportAsync(Guid userId, string? overrideEmail = null, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var settings = await _settingsRepository.GetByUserIdAsync(userId);

        var recipient = !string.IsNullOrWhiteSpace(overrideEmail)
            ? overrideEmail
            : user?.Email;

        if (string.IsNullOrWhiteSpace(recipient))
        {
            _logger.LogWarning("Cannot send weekly report: no valid email found for UserId: {UserId}", userId);
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var fromUtc = nowUtc.AddDays(-7);
        var sessions = await _pomodoroRepository.GetSessionHistoryAsync(userId, fromUtc, nowUtc);

        var totalPomos = sessions.Count(s => s.IsCompleted && s.Mode == FocusSessionMode.Pomo);
        var totalFocusSeconds = sessions.Where(s => s.IsCompleted).Sum(s => s.ActualSeconds);
        var totalOvertimeSeconds = sessions.Where(s => s.IsCompleted).Sum(s => s.OvertimeSeconds);
        var streak = await _pomodoroRepository.GetDailyStreakByUserIdAsync(userId);

        var completedTasks = await _taskRepository.GetUserTasksAsync(userId, new TaskFilterParams { Status = TaskStatus.Completed });
        var taskStats = completedTasks
            .Select(t => (t.Title, t.FocusSessions.Count(s => s.IsCompleted)))
            .ToList();

        var dateRangeText = $"{fromUtc:dd/MM} - {nowUtc:dd/MM/yyyy}";
        var displayName = user?.DisplayName ?? "bạn";
        var appUrl = "http://localhost:3000";

        var html = ReportEmailTemplates.BuildWeeklyReportHtml(
            displayName,
            dateRangeText,
            totalPomos,
            totalFocusSeconds / 60,
            totalOvertimeSeconds / 60,
            streak?.CurrentStreak ?? 0,
            taskStats,
            appUrl);

        await _emailService.SendEmailReportAsync(recipient, "[Pomo-Do] Báo cáo năng suất tuần của bạn", html, ct);

        if (settings != null)
        {
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            var diff = (7 + (vnTime.DayOfWeek - DayOfWeek.Monday)) % 7;
            settings.LastWeeklyReportSentDate = DateOnly.FromDateTime(vnTime.AddDays(-diff));
            await _settingsRepository.SaveChangesAsync();
        }
    }

    // Gửi tóm tắt công việc đầu ngày lúc 07:00 sáng
    public async Task SendDailyDigestAsync(Guid userId, string? overrideEmail = null, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var settings = await _settingsRepository.GetByUserIdAsync(userId);

        var recipient = !string.IsNullOrWhiteSpace(overrideEmail)
            ? overrideEmail
            : user?.Email;

        if (string.IsNullOrWhiteSpace(recipient))
        {
            _logger.LogWarning("Cannot send daily digest: no valid email found for UserId: {UserId}", userId);
            return;
        }

        var activeTasks = await _taskRepository.GetUserTasksAsync(userId, new TaskFilterParams { Status = TaskStatus.Todo });
        var urgentImportant = activeTasks.Where(t => t.Priority == Priority.ImportantUrgent).Select(t => t.Title).ToList();
        var important = activeTasks.Where(t => t.Priority == Priority.ImportantNotUrgent).Select(t => t.Title).ToList();
        var others = activeTasks.Where(t => t.Priority == Priority.NotImportantUrgent || t.Priority == Priority.NotImportantNotUrgent).Select(t => t.Title).ToList();

        var streak = await _pomodoroRepository.GetDailyStreakByUserIdAsync(userId);
        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        var dateText = vnTime.ToString("dd/MM/yyyy");
        var displayName = user?.DisplayName ?? "bạn";
        var appUrl = "http://localhost:3000";

        var html = ReportEmailTemplates.BuildDailyDigestHtml(
            displayName,
            dateText,
            streak?.CurrentStreak ?? 0,
            urgentImportant,
            important,
            others,
            appUrl);

        await _emailService.SendEmailReportAsync(recipient, "[Pomo-Do] Chào buổi sáng! Tóm tắt công việc hôm nay", html, ct);

        if (settings != null)
        {
            settings.LastDailyDigestSentDate = DateOnly.FromDateTime(vnTime);
            await _settingsRepository.SaveChangesAsync();
        }
    }

    // Quét và đẩy các job Daily Digest cần gửi vào Redis Streams
    public async Task EnqueueScheduledDailyDigestsAsync(string currentTime, CancellationToken ct = default)
    {
        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        var today = DateOnly.FromDateTime(vnTime);

        var targetUsers = await _settingsRepository.GetUsersForDailyDigestAsync(today, currentTime, ct);
        if (targetUsers.Count > 0)
        {
            _logger.LogInformation("Found {Count} users to enqueue for Daily Digest on {Date} (Cutoff: {Time})", targetUsers.Count, today, currentTime);
            foreach (var s in targetUsers)
            {
                await _reportQueue.EnqueueReportAsync(s.UserId, "Daily", null, ct);
                s.LastDailyDigestSentDate = today;
            }
            await _settingsRepository.SaveChangesAsync();
        }
    }

    // Quét và đẩy các job Weekly Report cần gửi vào Redis Streams
    public async Task EnqueueScheduledWeeklyReportsAsync(string currentTime, CancellationToken ct = default)
    {
        var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        var diff = (7 + (vnTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = DateOnly.FromDateTime(vnTime.AddDays(-diff));

        var targetUsers = await _settingsRepository.GetUsersForWeeklyReportAsync(monday, currentTime, ct);
        if (targetUsers.Count > 0)
        {
            _logger.LogInformation("Found {Count} users to enqueue for Weekly Report on week of {Date} (Cutoff: {Time})", targetUsers.Count, monday, currentTime);
            foreach (var s in targetUsers)
            {
                await _reportQueue.EnqueueReportAsync(s.UserId, "Weekly", null, ct);
                s.LastWeeklyReportSentDate = monday;
            }
            await _settingsRepository.SaveChangesAsync();
        }
    }
}
