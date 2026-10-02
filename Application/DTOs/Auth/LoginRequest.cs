using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

// Payload required for user login
public record LoginRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    string Password
);
