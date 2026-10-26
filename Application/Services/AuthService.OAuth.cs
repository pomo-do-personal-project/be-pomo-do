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
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
        }
        catch (Exception)
        {
            throw new DomainException("Google ID Token không hợp lệ hoặc đã hết hạn.");
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new DomainException("Không thể lấy email từ tài khoản Google.");
        }

        var normalizedEmail = payload.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null)
        {
            user = new User
            {
                Email = normalizedEmail,
                DisplayName = string.IsNullOrWhiteSpace(payload.Name) ? normalizedEmail.Split('@')[0] : payload.Name.Trim(),
                AvatarUrl = payload.Picture,
                Provider = UserProvider.Google,
                ProviderId = payload.Subject,
                PasswordHash = null,
                CreatedAt = DateTime.UtcNow,
                Settings = new UserSettings
                {
                    FocusDuration = 25,
                    ShortBreakDuration = 5,
                    LongBreakDuration = 15,
                    AutoStartBreaks = false,
                    ThemeMode = "light"
                },
                Streak = new DailyStreak { CurrentStreak = 0, LongestStreak = 0 }
            };
            user.Settings.UserId = user.Id;
            user.Streak.UserId = user.Id;

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

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = BuildRefreshToken(user.Id, refreshTokenString);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new AuthResponse(user.ToDto(), accessToken, refreshTokenString, _jwtOptions.AccessTokenExpirationMinutes * 60);
    }

    // Đăng nhập hoặc đăng ký tự động bằng Supabase Access Token
    public async Task<AuthResponse> SupabaseAuthAsync(SupabaseAuthRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            throw new DomainException("Supabase Access Token không được để trống.");
        }

        var supabaseUrl = "https://zbygkwrbjeyufepqvpsz.supabase.co/auth/v1/user";
        var supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InpieWdrd3JiamV5dWZlcHF2cHN6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTA4Mzg2ODksImV4cCI6MjEwNjQxNDY4OX0.Qs2ZqqE-RF8MsEh-sTD6GiR5pQPruIMqiJe_60nQz7s";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, supabaseUrl);
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
            user = new User
            {
                Email = normalizedEmail,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? normalizedEmail.Split('@')[0] : displayName.Trim(),
                AvatarUrl = avatarUrl,
                Provider = UserProvider.Google,
                ProviderId = id,
                PasswordHash = null,
                CreatedAt = DateTime.UtcNow,
                Settings = new UserSettings
                {
                    FocusDuration = 25,
                    ShortBreakDuration = 5,
                    LongBreakDuration = 15,
                    AutoStartBreaks = false,
                    ThemeMode = "light"
                },
                Streak = new DailyStreak { CurrentStreak = 0, LongestStreak = 0 }
            };
            user.Settings.UserId = user.Id;
            user.Streak.UserId = user.Id;

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

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = BuildRefreshToken(user.Id, refreshTokenString);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new AuthResponse(user.ToDto(), accessToken, refreshTokenString, _jwtOptions.AccessTokenExpirationMinutes * 60);
    }
}
