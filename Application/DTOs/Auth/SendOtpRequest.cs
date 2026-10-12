using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Auth;

// DTO yêu cầu gửi mã OTP đăng ký tài khoản
public record SendOtpRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    OtpPurpose Purpose = OtpPurpose.Register
);
