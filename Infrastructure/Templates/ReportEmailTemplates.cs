using System.Text;

namespace be_pomo_do.Infrastructure.Templates;

// Mẫu HTML email báo cáo định kỳ (Báo cáo tuần & Tóm tắt đầu ngày)
public static class ReportEmailTemplates
{
    // Tạo HTML cho Báo cáo năng suất tuần gửi sáng Thứ Hai
    public static string BuildWeeklyReportHtml(
        string displayName,
        string dateRangeText,
        int totalPomodoros,
        int totalFocusMinutes,
        int totalOvertimeMinutes,
        int currentStreak,
        IReadOnlyList<(string Title, int Pomodoros)> completedTasks,
        string appUrl)
    {
        var focusHours = totalFocusMinutes / 60;
        var focusMins = totalFocusMinutes % 60;
        var timeDisplay = focusHours > 0 ? $"{focusHours}h {focusMins}m" : $"{focusMins}m";

        var taskListHtml = new StringBuilder();
        if (completedTasks.Count > 0)
        {
            taskListHtml.Append("<div style=\"margin-top: 16px; border-radius: 8px; border: 1px solid #f1f5f9; background-color: #f8fafc; padding: 12px 16px;\">");
            foreach (var task in completedTasks.Take(8))
            {
                var safeTitle = System.Net.WebUtility.HtmlEncode(task.Title);
                taskListHtml.Append($"""
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 8px 0; border-bottom: 1px dashed #e2e8f0; font-size: 13px;">
                  <span style="color: #334155; font-weight: 500;">✓ {safeTitle}</span>
                  <span style="color: #64748b; font-size: 12px; background: #ffffff; padding: 2px 8px; border-radius: 9999px; border: 1px solid #e2e8f0;">🍅 {task.Pomodoros} pomo</span>
                </div>
                """);
            }
            if (completedTasks.Count > 8)
            {
                taskListHtml.Append($"<p style=\"margin: 8px 0 0 0; font-size: 12px; color: #94a3b8; text-align: center;\">+ {completedTasks.Count - 8} công việc khác đã hoàn thành</p>");
            }
            taskListHtml.Append("</div>");
        }
        else
        {
            taskListHtml.Append("<p style=\"margin: 12px 0; font-size: 13px; color: #94a3b8; font-style: italic;\">Chưa có công việc nào được đánh dấu hoàn thành trong tuần qua.</p>");
        }

        return $$"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Báo cáo năng suất tuần Pomo-Do</title>
        </head>
        <body style="margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #0f172a;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color: #f8fafc; padding: 40px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" width="100%" style="max-width: 520px; background-color: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; padding: 36px 32px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);">
                  <tr>
                    <td style="padding-bottom: 20px;">
                      <table width="100%">
                        <tr>
                          <td>
                            <span style="font-size: 16px; font-weight: 800; letter-spacing: -0.3px; color: #0f172a; text-transform: uppercase;">
                              POMO<span style="color: #e11d48;">-DO</span>
                            </span>
                          </td>
                          <td align="right">
                            <span style="font-size: 11px; font-weight: 600; background-color: #f1f5f9; color: #475569; padding: 4px 10px; border-radius: 9999px;">
                              {{dateRangeText}}
                            </span>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-bottom: 24px;">
                      <h1 style="margin: 0 0 8px 0; font-size: 22px; font-weight: 700; color: #0f172a; letter-spacing: -0.5px;">
                        📊 Báo cáo năng suất tuần
                      </h1>
                      <p style="margin: 0; font-size: 14px; line-height: 22px; color: #475569;">
                        Xin chào <strong>{{System.Net.WebUtility.HtmlEncode(displayName)}}</strong>, cùng nhìn lại thành quả rèn luyện sự tập trung của bạn trong tuần qua để sẵn sàng cho tuần mới đầy hứng khởi!
                      </p>
                    </td>
                  </tr>

                  <!-- METRIC STATS GRID -->
                  <tr>
                    <td style="padding-bottom: 28px;">
                      <table width="100%" cellspacing="0" cellpadding="0">
                        <tr>
                          <td width="48%" style="background-color: #fff1f2; border: 1px solid #ffe4e6; border-radius: 12px; padding: 16px; text-align: center;">
                            <div style="font-size: 11px; font-weight: 700; text-transform: uppercase; color: #e11d48; letter-spacing: 0.5px;">Phiên Pomodoro</div>
                            <div style="font-size: 28px; font-weight: 800; color: #be123c; margin-top: 4px;">{{totalPomodoros}}</div>
                            <div style="font-size: 11px; color: #9f1239; margin-top: 2px;">phiên hoàn thành</div>
                          </td>
                          <td width="4%"></td>
                          <td width="48%" style="background-color: #eff6ff; border: 1px solid #dbeafe; border-radius: 12px; padding: 16px; text-align: center;">
                            <div style="font-size: 11px; font-weight: 700; text-transform: uppercase; color: #2563eb; letter-spacing: 0.5px;">Thời gian tập trung</div>
                            <div style="font-size: 28px; font-weight: 800; color: #1d4ed8; margin-top: 4px;">{{timeDisplay}}</div>
                            <div style="font-size: 11px; color: #1e40af; margin-top: 2px;">+{{totalOvertimeMinutes}}m overtime</div>
                          </td>
                        </tr>
                      </table>

                      <div style="margin-top: 12px; background-color: #fefce8; border: 1px solid #fef08a; border-radius: 12px; padding: 12px 16px; display: flex; align-items: center; justify-content: space-between;">
                        <span style="font-size: 13px; font-weight: 600; color: #854d0e;">🔥 Chuỗi ngày liên tục (Streak):</span>
                        <span style="font-size: 15px; font-weight: 800; color: #a16207;">{{currentStreak}} ngày liên tiếp</span>
                      </div>
                    </td>
                  </tr>

                  <!-- COMPLETED TASKS -->
                  <tr>
                    <td style="padding-bottom: 28px;">
                      <h2 style="margin: 0; font-size: 15px; font-weight: 600; color: #1e293b;">
                        ✅ Công việc đã hoàn thành ({{completedTasks.Count}})
                      </h2>
                      {{taskListHtml}}
                    </td>
                  </tr>

                  <!-- CTA BUTTON -->
                  <tr>
                    <td style="padding-bottom: 28px; text-align: center;">
                      <a href="{{appUrl}}" target="_blank" style="display: inline-block; background-color: #e11d48; color: #ffffff; font-size: 14px; font-weight: 600; padding: 12px 28px; text-decoration: none; border-radius: 10px; box-shadow: 0 4px 6px -1px rgba(225, 29, 72, 0.25);">
                        Mở Pomo-Do lập kế hoạch tuần mới &rarr;
                      </a>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-top: 20px; border-top: 1px solid #f1f5f9; text-align: center;">
                      <p style="margin: 0; font-size: 12px; color: #94a3b8;">
                        Email này được gửi tự động từ <strong>Pomo-Do</strong> vì bạn đã bật nhận Báo cáo tuần.<br>
                        Bạn có thể điều chỉnh cấu hình này bất cứ lúc nào trong mục Cài đặt.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }

    // Tạo HTML cho Tóm tắt công việc đầu ngày gửi lúc 07:00 sáng
    public static string BuildDailyDigestHtml(
        string displayName,
        string dateText,
        int currentStreak,
        IReadOnlyList<string> urgentImportantTasks,
        IReadOnlyList<string> importantTasks,
        IReadOnlyList<string> otherTasks,
        string appUrl)
    {
        var totalTasks = urgentImportantTasks.Count + importantTasks.Count + otherTasks.Count;
        var tasksHtml = new StringBuilder();

        if (totalTasks == 0)
        {
            tasksHtml.Append("<div style=\"padding: 16px; background-color: #f8fafc; border-radius: 10px; border: 1px dashed #cbd5e1; text-align: center; color: #64748b; font-size: 13px;\">Hôm nay bạn chưa có công việc nào đang chờ. Hãy mở ứng dụng để thêm công việc mới nhé!</div>");
        }
        else
        {
            if (urgentImportantTasks.Count > 0)
            {
                tasksHtml.Append("<div style=\"margin-top: 12px;\"><div style=\"font-size: 12px; font-weight: 700; color: #dc2626; text-transform: uppercase;\">🔴 Ưu tiên số 1 (Quan trọng & Khẩn cấp)</div><ul style=\"margin: 6px 0 14px 0; padding-left: 20px; color: #334155; font-size: 13px;\">");
                foreach (var t in urgentImportantTasks.Take(5))
                {
                    tasksHtml.Append($"<li style=\"margin-bottom: 4px;\"><strong>{System.Net.WebUtility.HtmlEncode(t)}</strong></li>");
                }
                tasksHtml.Append("</ul></div>");
            }

            if (importantTasks.Count > 0)
            {
                tasksHtml.Append("<div style=\"margin-top: 10px;\"><div style=\"font-size: 12px; font-weight: 700; color: #d97706; text-transform: uppercase;\">🟡 Quan trọng (Cần tập trung hôm nay)</div><ul style=\"margin: 6px 0 14px 0; padding-left: 20px; color: #334155; font-size: 13px;\">");
                foreach (var t in importantTasks.Take(5))
                {
                    tasksHtml.Append($"<li style=\"margin-bottom: 4px;\">{System.Net.WebUtility.HtmlEncode(t)}</li>");
                }
                tasksHtml.Append("</ul></div>");
            }

            if (otherTasks.Count > 0)
            {
                tasksHtml.Append("<div style=\"margin-top: 10px;\"><div style=\"font-size: 12px; font-weight: 700; color: #2563eb; text-transform: uppercase;\">🔵 Các việc cần xử lý khác</div><ul style=\"margin: 6px 0 14px 0; padding-left: 20px; color: #334155; font-size: 13px;\">");
                foreach (var t in otherTasks.Take(5))
                {
                    tasksHtml.Append($"<li style=\"margin-bottom: 4px;\">{System.Net.WebUtility.HtmlEncode(t)}</li>");
                }
                tasksHtml.Append("</ul></div>");
            }
        }

        return $$"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Tóm tắt đầu ngày Pomo-Do</title>
        </head>
        <body style="margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #0f172a;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color: #f8fafc; padding: 40px 16px;">
            <tr>
              <td align="center">
                <table role="presentation" width="100%" style="max-width: 520px; background-color: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; padding: 36px 32px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);">
                  <tr>
                    <td style="padding-bottom: 20px;">
                      <table width="100%">
                        <tr>
                          <td>
                            <span style="font-size: 16px; font-weight: 800; letter-spacing: -0.3px; color: #0f172a; text-transform: uppercase;">
                              POMO<span style="color: #e11d48;">-DO</span>
                            </span>
                          </td>
                          <td align="right">
                            <span style="font-size: 11px; font-weight: 600; background-color: #fef3c7; color: #b45309; padding: 4px 10px; border-radius: 9999px;">
                              🌅 07:00 AM • {{dateText}}
                            </span>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-bottom: 20px;">
                      <h1 style="margin: 0 0 8px 0; font-size: 22px; font-weight: 700; color: #0f172a; letter-spacing: -0.5px;">
                        Chào buổi sáng, {{System.Net.WebUtility.HtmlEncode(displayName)}}!
                      </h1>
                      <p style="margin: 0; font-size: 14px; line-height: 22px; color: #475569;">
                        Hôm nay bạn có <strong>{{totalTasks}} công việc</strong> đang chờ hoàn thành. Hãy bắt đầu ngày mới thật tập trung nhé!
                      </p>
                    </td>
                  </tr>

                  <!-- STREAK BANNER -->
                  <tr>
                    <td style="padding-bottom: 24px;">
                      <div style="background-color: #fff7ed; border: 1px solid #ffedd5; border-radius: 12px; padding: 12px 16px;">
                        <span style="font-size: 13px; font-weight: 600; color: #c2410c;">🔥 Chuỗi ngày liên tục: <strong>{{currentStreak}} ngày</strong></span>
                        <div style="font-size: 12px; color: #9a3412; margin-top: 4px;">Hoàn thành ít nhất 1 phiên Pomodoro hôm nay để không bị đứt chuỗi nhé!</div>
                      </div>
                    </td>
                  </tr>

                  <!-- TASKS SECTION -->
                  <tr>
                    <td style="padding-bottom: 28px;">
                      <h2 style="margin: 0; font-size: 15px; font-weight: 600; color: #1e293b; border-bottom: 1px solid #f1f5f9; padding-bottom: 8px;">
                        📋 Danh sách công việc hôm nay
                      </h2>
                      {{tasksHtml}}
                    </td>
                  </tr>

                  <!-- CTA BUTTON -->
                  <tr>
                    <td style="padding-bottom: 28px; text-align: center;">
                      <a href="{{appUrl}}" target="_blank" style="display: inline-block; background-color: #e11d48; color: #ffffff; font-size: 14px; font-weight: 600; padding: 12px 28px; text-decoration: none; border-radius: 10px; box-shadow: 0 4px 6px -1px rgba(225, 29, 72, 0.25);">
                        Bắt đầu phiên Pomodoro đầu tiên &rarr;
                      </a>
                    </td>
                  </tr>

                  <tr>
                    <td style="padding-top: 20px; border-top: 1px solid #f1f5f9; text-align: center;">
                      <p style="margin: 0; font-size: 12px; color: #94a3b8;">
                        Email này được gửi tự động lúc 07:00 sáng từ <strong>Pomo-Do</strong>.<br>
                        Bạn có thể tắt hoặc điều chỉnh trong mục Cài đặt.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }
}
