using be_pomo_do.Application.Common.Mappings;
using be_pomo_do.Application.DTOs.Auth;
using be_pomo_do.Core.Entities;
using be_pomo_do.Core.Exceptions;
using be_pomo_do.Core.ValueObjects;
using Google.Apis.Auth;

namespace be_pomo_do.Application.Services;

// Phần mở rộng xử lý xác thực bên thứ ba (Google OAuth & Supabase Auth)
public partial class AuthService : IAuthService
{
    private static readonly HttpClient _supabaseHttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    // Đăng nhập hoặc đăng ký tự động bằng Google ID Token
    public async Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            throw new DomainException("Google ID Token không được để trống.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var googleClientId = _configuration["GoogleAuth:ClientId"];
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings();
            if (!string.IsNullOrWhiteSpace(googleClientId))
            {
                validationSettings.Audience = new[] { googleClientId };
            }
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
        }
        catch (Exception)
        {
            throw new DomainException("Google ID Token không hợp lệ hoặc đã hết hạn.");
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new DomainException("Không thể lấy email từ tài khoản Google.");
        }

        if (!payload.EmailVerified)
        {
            throw new DomainException("Email Google chưa được xác thực.");
        }

        var normalizedEmail = payload.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null)
        {
            var displayName = string.IsNullOrWhiteSpace(payload.Name) ? normalizedEmail.Split('@')[0] : payload.Name.Trim();
            user = CreateNewUser(
                email: normalizedEmail,
                displayName: displayName,
                provider: UserProvider.Google,
                providerId: payload.Subject,
                avatarUrl: payload.Picture);

            await _userRepository.AddAsync(user);
        }
        else
        {
            if (string.IsNullOrEmpty(user.ProviderId))
            {
                user.ProviderId = payload.Subject;
            }
            if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(payload.Picture))
            {
                user.AvatarUrl = payload.Picture;
            }
        }

        if (user.IsSuspended)
        {
            var reason = string.IsNullOrWhiteSpace(user.SuspendedReason) ? "Vi phạm điều khoản dịch vụ" : user.SuspendedReason;
            throw new DomainException($"Tài khoản của bạn đã bị tạm khóa. Lý do: {reason}");
        }

        return await IssueAuthResponseAsync(user);
    }

    // Đăng nhập hoặc đăng ký tự động bằng Supabase Access Token
    public async Task<AuthResponse> SupabaseAuthAsync(SupabaseAuthRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            throw new DomainException("Supabase Access Token không được để trống.");
        }

        var supabaseUrl = _configuration["Supabase:Url"] ?? "https://zbygkwrbjeyufepqvpsz.supabase.co";
        var supabaseAnonKey = _configuration["Supabase:AnonKey"] ?? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InpieWdrd3JiamV5dWZlcHF2cHN6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTA4Mzg2ODksImV4cCI6MjEwNjQxNDY4OX0.Qs2ZqqE-RF8MsEh-sTD6GiR5pQPruIMqiJe_60nQz7s";
        var userEndpoint = $"{supabaseUrl.TrimEnd('/')}/auth/v1/user";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, userEndpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.AccessToken);
        httpRequest.Headers.Add("apikey", supabaseAnonKey);

        HttpResponseMessage response;
        try
        {
            response = await _supabaseHttpClient.SendAsync(httpRequest);
        }
        catch (Exception ex)
        {
            throw new DomainException($"Không thể kết nối đến máy chủ Supabase Auth: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new DomainException("Phiên đăng nhập Supabase không hợp lệ hoặc đã hết hạn.");
        }

        var jsonContent = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Không tìm thấy email trong tài khoản Supabase.");
        }

        // Kiểm tra tài khoản Supabase đã xác nhận email (tránh giả mạo email để chiếm đoạt tài khoản)
        var isEmailConfirmed = false;
        if (root.TryGetProperty("email_confirmed_at", out var confirmedProp) && confirmedProp.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            isEmailConfirmed = !string.IsNullOrWhiteSpace(confirmedProp.GetString());
        }
        else if (root.TryGetProperty("confirmed_at", out var confProp) && confProp.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            isEmailConfirmed = !string.IsNullOrWhiteSpace(confProp.GetString());
        }
        else if (root.TryGetProperty("app_metadata", out var appMeta) && appMeta.TryGetProperty("provider", out var prov) && prov.GetString() == "google")
        {
            isEmailConfirmed = true;
        }

        if (!isEmailConfirmed)
        {
            throw new DomainException("Tài khoản Supabase chưa được xác nhận email.");
        }

        var id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        string? displayName = null;
        string? avatarUrl = null;

        if (root.TryGetProperty("user_metadata", out var meta))
        {
            if (meta.TryGetProperty("full_name", out var fn)) displayName = fn.GetString();
            else if (meta.TryGetProperty("name", out var n)) displayName = n.GetString();

            if (meta.TryGetProperty("avatar_url", out var av)) avatarUrl = av.GetString();
            else if (meta.TryGetProperty("picture", out var pic)) avatarUrl = pic.GetString();
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null)
        {
            var resolvedName = string.IsNullOrWhiteSpace(displayName) ? normalizedEmail.Split('@')[0] : displayName.Trim();
            user = CreateNewUser(
                email: normalizedEmail,
                displayName: resolvedName,
                provider: UserProvider.Google,
                providerId: id,
                avatarUrl: avatarUrl);

            await _userRepository.AddAsync(user);
        }
        else
        {
            if (string.IsNullOrEmpty(user.ProviderId) && !string.IsNullOrEmpty(id))
            {
                user.ProviderId = id;
            }
            if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(avatarUrl))
            {
                user.AvatarUrl = avatarUrl;
            }
        }

        if (user.IsSuspended)
        {
            var reason = string.IsNullOrWhiteSpace(user.SuspendedReason) ? "Vi phạm điều khoản dịch vụ" : user.SuspendedReason;
            throw new DomainException($"Tài khoản của bạn đã bị tạm khóa. Lý do: {reason}");
        }

        return await IssueAuthResponseAsync(user);
    }
}
