namespace Common.Configuration;

public class AppConfig
{
    public string AspNetCoreEnvironment { get; set; } = "Development";
    public string CorsOrigins { get; set; } = string.Empty;
    public string LlmProvider { get; set; } = "mistral";
    public string LlmApiKey { get; set; } = string.Empty;
    public string LlmBaseUrl { get; set; } = "https://api.mistral.ai";
    public string LlmChatModel { get; set; } = "mistral-small-latest";
}