using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>
/// Represents the payload required for user login.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <param name="Password">The plain-text password for authentication.</param>
public record LoginRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    string Password
);
