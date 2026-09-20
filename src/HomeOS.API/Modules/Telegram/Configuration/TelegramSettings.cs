namespace HomeOS.API.Modules.Telegram.Configuration;

public class TelegramSettings
{
    public const string Section = "Telegram";

    public string BotToken { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    public string ApiBaseUrl => $"https://api.telegram.org/bot{BotToken}";
}
