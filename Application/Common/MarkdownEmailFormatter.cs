using System.Net;
using System.Text.RegularExpressions;

namespace be_pomo_do.Application.Common;

// Chuyển đổi cú pháp markdown soạn thảo từ Admin sang HTML an toàn, chuẩn email
public static class MarkdownEmailFormatter
{
    public static string FormatToSafeHtml(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        // 1. Mã hóa HTML entities để ngăn chặn tấn công chèn HTML/XSS vào email client
        var html = WebUtility.HtmlEncode(rawText);

        // 2. Chuyển đổi tiêu đề # H1, ## H2
        html = Regex.Replace(html, @"(?m)^#\s+(.+)$", "<h2 style='margin:12px 0 6px;color:#111;'>$1</h2>");
        html = Regex.Replace(html, @"(?m)^##\s+(.+)$", "<h3 style='margin:10px 0 4px;color:#333;'>$1</h3>");

        // 3. Trích dẫn Blockquote (> quote)
        html = Regex.Replace(
            html,
            @"(?m)^&gt;\s+(.+)$",
            "<blockquote style='border-left:3px solid #d1d5db;margin:6px 0;padding-left:10px;color:#4b5563;font-style:italic;'>$1</blockquote>"
        );

        // 4. In đậm (**text**)
        html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");

        // 5. Gạch ngang (~~text~~)
        html = Regex.Replace(html, @"~~(.+?)~~", "<del>$1</del>");

        // 6. In nghiêng (*text*) - không bắt nhầm cụm **
        html = Regex.Replace(html, @"(?<!\*)\*([^\*\n]+?)\*(?!\*)", "<em>$1</em>");

        // 7. Khôi phục thẻ <u> sau khi đã escape
        html = html.Replace("&lt;u&gt;", "<u style='text-decoration:underline;'>")
                   .Replace("&lt;/u&gt;", "</u>");

        // 8. Chuyển đổi xuống dòng thành <br/>
        html = html.Replace("\r\n", "<br/>").Replace("\n", "<br/>");

        return $"<div style='font-family:-apple-system,BlinkMacSystemFont,\"Segoe UI\",Roboto,sans-serif;font-size:14px;line-height:1.6;color:#1f2937;'>{html}</div>";
    }
}
