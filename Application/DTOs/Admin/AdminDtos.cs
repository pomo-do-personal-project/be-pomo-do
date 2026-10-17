namespace be_pomo_do.Application.DTOs.Admin;

public record AdminUserSummaryDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTime CreatedAt,
    bool IsAdmin,
    bool IsSuspended,
    string? SuspendedReason,
    DateTime? SuspendedAt,
    DateTime? LastActiveAt
);

public record AdminMetricsDto(
    int TotalUsers,
    int ActiveUsersToday,
    int SuspendedUsers,
    double ActiveRatePercentage,
    int GoogleUsersCount,
    int LocalUsersCount
);

public record ServiceStatusDto(
    string Name,
    string Status, // "healthy" | "degraded" | "offline"
    int LatencyMs,
    string Details
);

public record AdminSystemHealthDto(
    ServiceStatusDto ApiBackend,
    ServiceStatusDto Database,
    ServiceStatusDto RedisCache,
    ServiceStatusDto AiVision
);

public record SystemBroadcastDto(
    string Message,
    string Type, // "info" | "warning" | "maintenance"
    bool IsActive,
    DateTime UpdatedAt
);

public record UpdateBroadcastRequest(
    string Message,
    string Type,
    bool IsActive
);

public record UpdateUserStatusRequest(
    bool IsSuspended,
    string? Reason
);

public record AdminAuditLogDto(
    Guid Id,
    Guid AdminId,
    string AdminEmail,
    string AdminDisplayName,
    Guid TargetUserId,
    string TargetUserEmail,
    string TargetUserDisplayName,
    string Action,
    string? Reason,
    string? Details,
    DateTime CreatedAt
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record AdminSendEmailRequest(
    List<Guid>? UserIds,
    List<string>? Emails,
    string Subject,
    string Content
);

public record AdminScheduledNotificationDto(
    Guid Id,
    Guid AdminId,
    string Title,
    string Content,
    string Category,
    string Scope,
    string? RecipientEmails,
    DateTime ScheduledAt,
    bool IsSent,
    DateTime? SentAt,
    DateTime CreatedAt
);

public record CreateScheduledNotificationRequest(
    string Title,
    string Content,
    string Category,
    string Scope,
    List<string>? RecipientEmails,
    DateTime ScheduledAtUtc
);

