using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

// DTO yêu cầu đặt lại mật khẩu mới bằng token từ email
public record ResetPasswordRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mã xác thực hoặc token không được để trống")]
    string Token,

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu mới phải có tối thiểu 8 ký tự")]
    string NewPassword
);
