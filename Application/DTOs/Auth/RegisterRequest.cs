using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>
/// Represents the payload required to register a new user.
/// </summary>
/// <param name="Email">The unique email address for the user.</param>
/// <param name="Password">The plain-text password for the user, minimum 8 characters.</param>
/// <param name="DisplayName">The user's display name, maximum 100 characters.</param>
public record RegisterRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có tối thiểu 8 ký tự")]
    string Password,

    [Required(ErrorMessage = "Tên hiển thị không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên hiển thị tối đa 100 ký tự")]
    string DisplayName
);
