using be_pomo_do.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace be_pomo_do.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<DailyStreak> DailyStreaks => Set<DailyStreak>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Subtask> Subtasks => Set<Subtask>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<FocusSession> FocusSessions => Set<FocusSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AdminActionHistory> AdminActionHistories => Set<AdminActionHistory>();
    public DbSet<ScheduledNotification> ScheduledNotifications => Set<ScheduledNotification>();
    public DbSet<SystemAsset> SystemAssets => Set<SystemAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Provider).HasColumnName("provider").HasConversion<string>();
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin").HasDefaultValue(false);
            entity.Property(e => e.IsSuspended).HasColumnName("is_suspended").HasDefaultValue(false);
            entity.Property(e => e.SuspendedReason).HasColumnName("suspended_reason").HasMaxLength(500);
            entity.Property(e => e.SuspendedAt).HasColumnName("suspended_at");

            entity.HasOne(e => e.Settings).WithOne(s => s.User).HasForeignKey<UserSettings>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Streak).WithOne(s => s.User).HasForeignKey<DailyStreak>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Tasks).WithOne(t => t.User).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Tags).WithOne(t => t.User).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.FocusSessions).WithOne(f => f.User).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.RefreshTokens).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.PerformedAdminActions).WithOne(a => a.AdminUser).HasForeignKey(a => a.AdminId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.ReceivedAdminActions).WithOne(a => a.TargetUser).HasForeignKey(a => a.TargetUserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.ScheduledNotifications).WithOne(s => s.AdminUser).HasForeignKey(s => s.AdminId).OnDelete(DeleteBehavior.Restrict);
        });

        // 2. UserSettings
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.ToTable("user_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.FocusDuration).HasColumnName("focus_duration").HasDefaultValue(25);
            entity.Property(e => e.ShortBreakDuration).HasColumnName("short_break_duration").HasDefaultValue(5);
            entity.Property(e => e.LongBreakDuration).HasColumnName("long_break_duration").HasDefaultValue(15);
            entity.Property(e => e.AutoStartBreaks).HasColumnName("auto_start_breaks").HasDefaultValue(false);
            entity.Property(e => e.ThemeMode).HasColumnName("theme_mode").HasMaxLength(50).HasDefaultValue("light");
            entity.Property(e => e.EmailWeeklyReport).HasColumnName("email_weekly_report").HasDefaultValue(false);
            entity.Property(e => e.EmailDailyDigest).HasColumnName("email_daily_digest").HasDefaultValue(false);
            entity.Property(e => e.ReportEmail).HasColumnName("report_email").HasMaxLength(255);
            entity.Property(e => e.DailyDigestTime).HasColumnName("daily_digest_time").HasMaxLength(10).HasDefaultValue("07:00");
            entity.Property(e => e.WeeklyReportTime).HasColumnName("weekly_report_time").HasMaxLength(10).HasDefaultValue("08:00");
            entity.Property(e => e.LastDailyDigestSentDate).HasColumnName("last_daily_digest_sent_date");
            entity.Property(e => e.LastWeeklyReportSentDate).HasColumnName("last_weekly_report_sent_date");
            entity.Property(e => e.WallpaperUrl).HasColumnName("wallpaper_url");
            entity.Property(e => e.AlarmSoundUrl).HasColumnName("alarm_sound_url");
            entity.Property(e => e.AmbientSoundUrl).HasColumnName("ambient_sound_url");
        });

        // 3. DailyStreaks
        modelBuilder.Entity<DailyStreak>(entity =>
        {
            entity.ToTable("daily_streaks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.CurrentStreak).HasColumnName("current_streak").HasDefaultValue(0);
            entity.Property(e => e.LongestStreak).HasColumnName("longest_streak").HasDefaultValue(0);
            entity.Property(e => e.LastActiveDate).HasColumnName("last_active_date");
        });

        // 4. Tasks
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Priority).HasColumnName("priority").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.EstimatedPomodoros).HasColumnName("estimated_pomodoros").HasDefaultValue(1);
            entity.Property(e => e.EstimatedMinutes).HasColumnName("estimated_minutes").HasDefaultValue(25);
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.IsQuickAdd).HasColumnName("is_quick_add").HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasMany(e => e.Subtasks).WithOne(s => s.Task).HasForeignKey(s => s.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.FocusSessions).WithOne(f => f.Task).HasForeignKey(f => f.TaskId).OnDelete(DeleteBehavior.SetNull);
        });

        // 5. Subtasks
        modelBuilder.Entity<Subtask>(entity =>
        {
            entity.ToTable("subtasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TaskId).HasColumnName("task_id").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed").HasDefaultValue(false);
            entity.Property(e => e.OrderIndex).HasColumnName("order_index").HasDefaultValue(0);
        });

        // 6. Tags
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ColorHex).HasColumnName("color_hex").HasMaxLength(7).HasDefaultValue("#3B82F6");
        });

        // 7. TaskTags
        modelBuilder.Entity<TaskTag>(entity =>
        {
            entity.ToTable("task_tags");
            entity.HasKey(e => new { e.TaskId, e.TagId });
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");

            entity.HasOne(e => e.Task).WithMany(t => t.TaskTags).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag).WithMany(t => t.TaskTags).HasForeignKey(e => e.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        // 8. FocusSessions
        modelBuilder.Entity<FocusSession>(entity =>
        {
            entity.ToTable("focus_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.Mode).HasColumnName("mode").HasConversion<string>();
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes").HasDefaultValue(25);
            entity.Property(e => e.ActualSeconds).HasColumnName("actual_seconds").HasDefaultValue(0);
            entity.Property(e => e.OvertimeSeconds).HasColumnName("overtime_seconds").HasDefaultValue(0);
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed").HasDefaultValue(true);
            entity.Property(e => e.StartedAt).HasColumnName("started_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
        });

        // 9. RefreshTokens
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
            entity.Property(e => e.ReplacedByToken).HasColumnName("replaced_by_token");
        });

        // 10. AdminActionHistories
        modelBuilder.Entity<AdminActionHistory>(entity =>
        {
            entity.ToTable("admin_action_histories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id").IsRequired();
            entity.Property(e => e.TargetUserId).HasColumnName("target_user_id").IsRequired();
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
            entity.Property(e => e.Details).HasColumnName("details").HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.AdminId);
            entity.HasIndex(e => e.TargetUserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // 11. ScheduledNotifications
        modelBuilder.Entity<ScheduledNotification>(entity =>
        {
            entity.ToTable("scheduled_notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").HasColumnType("text").IsRequired();
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50).HasDefaultValue("info");
            entity.Property(e => e.Scope).HasColumnName("scope").HasMaxLength(50).HasDefaultValue("direct");
            entity.Property(e => e.RecipientEmails).HasColumnName("recipient_emails").HasColumnType("text");
            entity.Property(e => e.ScheduledAt).HasColumnName("scheduled_at").IsRequired();
            entity.Property(e => e.IsSent).HasColumnName("is_sent").HasDefaultValue(false);
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.IsSent, e.ScheduledAt });
            entity.HasIndex(e => e.AdminId);
        });

        // 12. SystemAssets
        modelBuilder.ApplyConfiguration(new Configurations.SystemAssetConfiguration());
    }
}
