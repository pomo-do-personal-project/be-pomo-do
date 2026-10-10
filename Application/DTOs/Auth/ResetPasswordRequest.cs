using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>Request payload to finalize resetting a user password using a verified OTP.</summary>
public record ResetPasswordRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mã OTP không được để trống")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số")]
    string OtpCode,

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu mới phải có tối thiểu 8 ký tự")]
    string NewPassword
);
