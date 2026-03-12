namespace Sqordia.Contracts.Requests.Newsletter;

public class SubscribeRequest
{
    public required string Email { get; set; }
    public string Language { get; set; } = "fr";
}
