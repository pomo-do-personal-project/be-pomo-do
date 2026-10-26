using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

// DTO yêu cầu xác thực bằng Supabase Access Token
public record SupabaseAuthRequest(
    [Required(ErrorMessage = "Supabase Access Token không được để trống")]
    string AccessToken
);
