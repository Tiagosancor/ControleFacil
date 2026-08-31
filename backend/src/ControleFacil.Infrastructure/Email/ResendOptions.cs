namespace ControleFacil.Infrastructure.Email;

public class ResendOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "onboarding@resend.dev";
    public string FromName { get; set; } = "Semeia Grana";
    public string ContactRecipientEmail { get; set; } = string.Empty;
}
