namespace be_pomo_do.Infrastructure.Services;

/// <summary>Strongly-typed configuration options for SMTP mail delivery.</summary>
public class SmtpOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "SmtpSettings";

    /// <summary>Gets or sets the SMTP host server address.</summary>
    public string Server { get; set; } = "smtp.gmail.com";

    /// <summary>Gets or sets the SMTP port number.</summary>
    public int Port { get; set; } = 587;

    /// <summary>Gets or sets the friendly display name of the email sender.</summary>
    public string SenderName { get; set; } = "Pomo-Do";

    /// <summary>Gets or sets the sender email address.</summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the username for SMTP authentication.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the password or app password for SMTP authentication.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets a value that indicates whether SSL/TLS encryption is enabled.</summary>
    public bool EnableSsl { get; set; } = true;
}
