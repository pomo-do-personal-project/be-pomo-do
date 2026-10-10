namespace be_pomo_do.Core.ValueObjects;

/// <summary>Defines the supported purposes for one-time password (OTP) verification.</summary>
public enum OtpPurpose
{
    /// <summary>OTP generated to verify email address when creating a new account.</summary>
    Register = 1,

    /// <summary>OTP generated to verify user identity when recovering a forgotten password.</summary>
    ForgotPassword = 2
}
