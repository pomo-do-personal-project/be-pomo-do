using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>Request payload required to register a new user account with an OTP verification code.</summary>
public record RegisterRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có tối thiểu 8 ký tự")]
    string Password,

    [Required(ErrorMessage = "Tên hiển thị không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên hiển thị tối đa 100 ký tự")]
    string DisplayName,

    [Required(ErrorMessage = "Mã OTP không được để trống")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số")]
    string OtpCode
);
