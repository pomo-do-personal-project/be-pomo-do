using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Auth;

/// <summary>Request payload to send an OTP code for either registration or password recovery.</summary>
public record SendOtpRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mục đích yêu cầu OTP không được để trống")]
    OtpPurpose Purpose
);
