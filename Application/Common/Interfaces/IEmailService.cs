using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Contract for dispatching transactional email messages.</summary>
public interface IEmailService
{
    /// <summary>Sends an OTP verification email containing a stylized HTML message and security advisory.</summary>
    /// <param name="toEmail">The destination recipient email.</param>
    /// <param name="otpCode">The 6-digit one-time password code.</param>
    /// <param name="purpose">The verification context (Register or ForgotPassword).</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendOtpEmailAsync(string toEmail, string otpCode, OtpPurpose purpose, CancellationToken ct = default);
}
