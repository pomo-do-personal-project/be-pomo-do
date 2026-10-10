using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.Common.Interfaces;

/// <summary>Contract for managing generation, lifecycle, and verification of one-time passwords via Redis.</summary>
public interface IOtpService
{
    /// <summary>Generates a cryptographically secure 6-digit OTP, stores it in Redis with a 5-minute TTL, and enforces cooldown.</summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="purpose">The purpose of the OTP (Register or ForgotPassword).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The generated 6-digit OTP string.</returns>
    Task<string> GenerateAndSaveOtpAsync(string email, OtpPurpose purpose, CancellationToken ct = default);

    /// <summary>Verifies whether the provided OTP code matches the stored value and consumes it if valid.</summary>
    /// <param name="email">The email associated with the OTP.</param>
    /// <param name="otpCode">The 6-digit code submitted by the user.</param>
    /// <param name="purpose">The verification purpose.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true" /> if verification succeeds; otherwise, <see langword="false" />.</returns>
    Task<bool> VerifyOtpAsync(string email, string otpCode, OtpPurpose purpose, CancellationToken ct = default);

    /// <summary>Gets the remaining cooldown time in seconds before a new OTP can be requested.</summary>
    /// <param name="email">The target email address.</param>
    /// <param name="purpose">The request purpose.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Remaining cooldown seconds (0 if cooldown has expired).</returns>
    Task<int> GetRemainingCooldownSecondsAsync(string email, OtpPurpose purpose, CancellationToken ct = default);
}
