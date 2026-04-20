namespace Tests.Llm;

using Common.Llm;
using Moq;
using Xunit;

public class IMistralClientContractTests
{
    [Fact]
    public async Task CompleteAsync_WithValidRequest_ReturnsResponseString()
    {
        var mockClient = new Moq.Mock<IMistralClient>();
        var systemPrompt = "You are a helpful assistant.";
        var userMessage = "Hello, world!";
        var expectedResponse = "{\"numero_siret\":\"12345678901234\",\"numero_engagement\":\"5678\",\"code_service\":\"SERV1\"}";

        mockClient.Setup(c => c.CompleteAsync(
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        var result = await mockClient.Object.CompleteAsync(systemPrompt, userMessage, null);

        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task CompleteAsync_ReceivesCorrectSystemPrompt()
    {
        var mockClient = new Moq.Mock<IMistralClient>();
        string capturedSystemPrompt = null!;
        string capturedUserMessage = null!;
        object? capturedResponseFormat = null;

        mockClient.Setup(c => c.CompleteAsync(
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<string>(),
            Moq.It.IsAny<object>()))
            .Callback<string, string, object?>((sys, user, fmt) =>
            {
                capturedSystemPrompt = sys;
                capturedUserMessage = user;
                capturedResponseFormat = fmt;
            })
            .ReturnsAsync("{}");

        await mockClient.Object.CompleteAsync("my-system-prompt", "my-user-message", "my-format");

        Assert.Equal("my-system-prompt", capturedSystemPrompt);
        Assert.Equal("my-user-message", capturedUserMessage);
        Assert.Equal("my-format", capturedResponseFormat);
    }

    [Fact]
    public async Task ExtractOcrAsync_ReturnsMarkdownText()
    {
        var mockClient = new Moq.Mock<IMistralClient>();
        var expectedOcrText = "OCR extracted text with Numéro de SIRET : 12345678901234";

        mockClient.Setup(c => c.ExtractOcrAsync(
            Moq.It.IsAny<byte[]>(),
            Moq.It.IsAny<string>()))
            .ReturnsAsync(expectedOcrText);

        var result = await mockClient.Object.ExtractOcrAsync(new byte[] { 1, 2, 3 }, "test.pdf");

        Assert.Equal(expectedOcrText, result);
    }
}
