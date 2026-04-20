namespace Tests.Llm;

using Common.Configuration;
using Common.Llm;
using Xunit;

public class LlmClientFactoryTests
{
    [Fact]
    public void Create_MistralProvider_ReturnsMistralLlmClient()
    {
        var config = new AppConfig
        {
            LlmProvider = "mistral",
            LlmApiKey = "test-key",
            LlmBaseUrl = "https://api.mistral.ai",
            LlmChatModel = "mistral-small-latest"
        };

        var client = LlmClientFactory.Create(config);

        Assert.IsType<MistralClient>(client);
    }

    [Fact]
    public void Create_MistralProvider_CreatesClientWithCorrectConfig()
    {
        var config = new AppConfig
        {
            LlmProvider = "mistral",
            LlmApiKey = "my-key",
            LlmBaseUrl = "https://custom.mistral.ai",
            LlmChatModel = "mistral-large-latest"
        };

        var client = (MistralClient)LlmClientFactory.Create(config);

        Assert.Equal("my-key", client.ApiKey);
        Assert.Equal("https://custom.mistral.ai", client.BaseUrl);
        Assert.Equal("mistral-large-latest", client.ChatModel);
    }

    [Fact]
    public void Create_UnsupportedProvider_ThrowsNotSupportedException()
    {
        var config = new AppConfig
        {
            LlmProvider = "openai",
            LlmApiKey = "test-key",
            LlmBaseUrl = "https://api.openai.com",
            LlmChatModel = "gpt-4o"
        };

        var ex = Assert.Throws<NotSupportedException>(() => LlmClientFactory.Create(config));
        Assert.Contains("Unsupported LLM provider: openai", ex.Message);
    }

    [Fact]
    public void Create_NullProvider_ThrowsNotSupportedException()
    {
        var config = new AppConfig
        {
            LlmProvider = null!,
            LlmApiKey = "test-key",
            LlmBaseUrl = "https://api.mistral.ai",
            LlmChatModel = "mistral-small-latest"
        };

        Assert.Throws<NotSupportedException>(() => LlmClientFactory.Create(config));
    }
}