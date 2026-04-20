namespace Common.Llm;

using Common.Configuration;

public static class LlmClientFactory
{
    public static IMistralClient Create(AppConfig config)
    {
        var provider = config.LlmProvider?.ToLowerInvariant() ?? "";

        return provider switch
        {
            "mistral" => new MistralClient(
                new HttpClient(),
                config.LlmApiKey ?? throw new InvalidOperationException("LlmApiKey not configured"),
                config.LlmBaseUrl ?? "https://api.mistral.ai",
                config.LlmChatModel ?? "mistral-small-latest"),
            _ => throw new NotSupportedException($"Unsupported LLM provider: {provider}")
        };
    }
}
