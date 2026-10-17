using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Auth;

// Request payload initiating password recovery verification for an existing user account.
public record ForgotPasswordRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    string Email
);
