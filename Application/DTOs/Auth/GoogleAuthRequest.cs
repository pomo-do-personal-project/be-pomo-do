using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

// DTO yêu cầu xác thực bằng Google ID Token
public record GoogleAuthRequest(
    [Required(ErrorMessage = "Google ID Token không được để trống")]
    string IdToken
);
