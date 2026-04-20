namespace Tests.Extraction;

using Common.Extraction;
using Common.Llm;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class FieldExtractorTests
{
    private static readonly object JsonSchemaFormat = FieldExtractor.BuildDocumentAnnotationSchema();

    [Fact]
    public void Extract_SiretOnly_ReturnsSiret()
    {
        var markdown = "Numéro de SIRET : 12345678901234\nAutre texte";

        var result = FieldExtractor.Extract(markdown);

        Assert.Equal("12345678901234", result.NumeroSiret);
        Assert.Null(result.NumeroEngagement);
        Assert.Null(result.CodeService);
    }

    [Fact]
    public void Extract_FullDocument_ReturnsAllFields()
    {
        var markdown = """
        Document fiscal
        Numéro de SIRET : 12345678901234
        Numéro Engagement : 5678
        Code de Service : SERV1
        """;

        var result = FieldExtractor.Extract(markdown);

        Assert.Equal("12345678901234", result.NumeroSiret);
        Assert.Equal("5678", result.NumeroEngagement);
        Assert.Equal("SERV1", result.CodeService);
    }

    [Fact]
    public void Extract_NoFields_ReturnsNulls()
    {
        var markdown = "Document sans champs";

        var result = FieldExtractor.Extract(markdown);

        Assert.Null(result.NumeroSiret);
        Assert.Null(result.NumeroEngagement);
        Assert.Null(result.CodeService);
    }

    [Fact]
    public async Task ExtractWithFallbackAsync_LlmSucceeds_ReturnsLlmFields()
    {
        var mockClient = new Mock<IMistralClient>();
        mockClient
            .Setup(c => c.CompleteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync("{\"numero_siret\":\"111\",\"numero_engagement\":\"222\",\"code_service\":\"SERV3\"}");

        var logger = new Mock<ILogger>();

        var (fields, method) = await FieldExtractor.ExtractWithFallbackAsync(
            "some markdown", mockClient.Object, logger.Object);

        Assert.Equal("111", fields.NumeroSiret);
        Assert.Equal("222", fields.NumeroEngagement);
        Assert.Equal("SERV3", fields.CodeService);
        Assert.Equal("llm", method);
    }

    [Fact]
    public async Task ExtractWithFallbackAsync_LlmFails_FallsBackToRegex()
    {
        var mockClient = new Mock<IMistralClient>();
        mockClient
            .Setup(c => c.CompleteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ThrowsAsync(new InvalidOperationException("API error"));

        var markdown = "Numéro de SIRET : 999\nNuméro Engagement : 888";
        var logger = new Mock<ILogger>();

        var (fields, method) = await FieldExtractor.ExtractWithFallbackAsync(
            markdown, mockClient.Object, logger.Object);

        Assert.Equal("999", fields.NumeroSiret);
        Assert.Equal("888", fields.NumeroEngagement);
        Assert.Equal("regex", method);
    }

    [Fact]
    public async Task ExtractWithFallbackAsync_LlmSucceeds_LogsSuccess()
    {
        var mockClient = new Mock<IMistralClient>();
        mockClient
            .Setup(c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync("{\"numero_siret\":\"111\",\"numero_engagement\":\"222\",\"code_service\":null}");

        var logger = new Mock<ILogger>();

        await FieldExtractor.ExtractWithFallbackAsync("markdown", mockClient.Object, logger.Object);

        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("LLM extraction succeeded")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExtractWithFallbackAsync_LlmFails_LogsWarning()
    {
        var mockClient = new Mock<IMistralClient>();
        mockClient
            .Setup(c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new InvalidOperationException("API error"));

        var logger = new Mock<ILogger>();

        await FieldExtractor.ExtractWithFallbackAsync("markdown", mockClient.Object, logger.Object);

        logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("LLM extraction failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void BuildDocumentAnnotationSchema_ReturnsValidJsonSchema()
    {
        var schema = FieldExtractor.BuildDocumentAnnotationSchema();
        var json = JsonSerializer.Serialize(schema);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        Assert.Equal("json_schema", root.GetProperty("type").GetString());
        Assert.Equal("invoice_fields", root.GetProperty("json_schema").GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExtractFromDocumentAsync_CallsOcrThenLlm()
    {
        var mockClient = new Mock<IMistralClient>();
        mockClient
            .Setup(c => c.ExtractOcrAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
            .ReturnsAsync("Numéro de SIRET : 123\nNuméro Engagement : 456");
        mockClient
            .Setup(c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync("{\"numero_siret\":\"123\",\"numero_engagement\":\"456\",\"code_service\":null}");

        var (fields, ocrText, extractionMethod) = await FieldExtractor.ExtractFromDocumentAsync(
            new byte[] { 1, 2, 3 }, "test.pdf", mockClient.Object);

        Assert.Equal("123", fields.NumeroSiret);
        Assert.Equal("456", fields.NumeroEngagement);
        Assert.Equal("llm", extractionMethod);
        Assert.Contains("Numéro de SIRET", ocrText);

        mockClient.Verify(c => c.ExtractOcrAsync(It.IsAny<byte[]>(), "test.pdf"), Times.Once);
        mockClient.Verify(c => c.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }
}
