namespace be_pomo_do.Infrastructure.Services;

// Strongly-typed configuration options for SMTP mail delivery.
public class SmtpOptions
{
    // Gets the configuration section name.
    public const string SectionName = "SmtpSettings";

    // Gets or sets the SMTP host server address.
    public string Server { get; set; } = "smtp.gmail.com";

    // Gets or sets the SMTP port number.
    public int Port { get; set; } = 587;

    // Gets or sets the friendly display name of the email sender.
    public string SenderName { get; set; } = "Pomo-Do";

    // Gets or sets the sender email address.
    public string SenderEmail { get; set; } = string.Empty;

    // Gets or sets the username for SMTP authentication.
    public string Username { get; set; } = string.Empty;

    // Gets or sets the password or app password for SMTP authentication.
    public string Password { get; set; } = string.Empty;

    // Gets or sets a value that indicates whether SSL/TLS encryption is enabled.
    public bool EnableSsl { get; set; } = true;

    // Gets or sets the base frontend URL for reset password button link.
    public string FrontendUrl { get; set; } = "https://localhost:3000";
}
