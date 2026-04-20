namespace Tests.Llm;

using System.Net;
using System.Net.Http;
using Common.Llm;
using Moq;
using Moq.Protected;
using Xunit;

public class MistralClientTests
{
    private static MistralClient CreateClient(HttpMessageHandler handler, string apiKey = "test-key", string baseUrl = "https://api.mistral.ai", string chatModel = "mistral-small-latest")
    {
        var httpClient = new HttpClient(handler);
        return new MistralClient(httpClient, apiKey, baseUrl, chatModel);
    }

    [Fact]
    public async Task CompleteAsync_WithContentResponse_ReturnsContentString()
    {
        var responseJson = """
        {
          "id": "test-id",
          "choices": [
            {
              "message": {
                "content": "{\"numero_siret\":\"12345678901234\",\"numero_engagement\":\"5678\",\"code_service\":\"SERV1\"}"
              }
            }
          ]
        }
        """;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var client = CreateClient(handlerMock.Object);
        var result = await client.CompleteAsync("system", "user", null);

        Assert.Equal("{\"numero_siret\":\"12345678901234\",\"numero_engagement\":\"5678\",\"code_service\":\"SERV1\"}", result);
    }

    [Fact]
    public async Task CompleteAsync_WithToolCallsResponse_ReturnsFunctionArguments()
    {
        var responseJson = """
        {
          "id": "test-id",
          "choices": [
            {
              "message": {
                "tool_calls": [
                  {
                    "function": {
                      "arguments": "{\"numero_siret\":\"98765432109876\",\"numero_engagement\":\"1111\",\"code_service\":\"SERV2\"}"
                    }
                  }
                ]
              }
            }
          ]
        }
        """;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var client = CreateClient(handlerMock.Object);
        var result = await client.CompleteAsync("system", "user", null);

        Assert.Equal("{\"numero_siret\":\"98765432109876\",\"numero_engagement\":\"1111\",\"code_service\":\"SERV2\"}", result);
    }

    [Fact]
    public async Task CompleteAsync_NonSuccessStatusCode_ThrowsInvalidOperationException()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\": \"invalid request\"}")
            });

        var client = CreateClient(handlerMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.CompleteAsync("system", "user", null));

        Assert.Contains("Mistral Chat API failed with StatusCode 400", ex.Message);
    }

    [Fact]
    public async Task CompleteAsync_UnexpectedResponseStructure_ThrowsInvalidOperationException()
    {
        var responseJson = """
        {
          "id": "test-id",
          "choices": [
            {
              "message": {
                "role": "assistant"
              }
            }
          ]
        }
        """;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var client = CreateClient(handlerMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.CompleteAsync("system", "user", null));

        Assert.Contains("Unexpected Mistral response structure", ex.Message);
    }

    [Fact]
    public async Task CompleteAsync_ValidRequest_SendsCorrectHeaders()
    {
        HttpRequestMessage? capturedRequest = null;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"id":"test","choices":[{"message":{"content":"{}"}}]}""")
            });

        var client = CreateClient(handlerMock.Object, "my-api-key", "https://custom.mistral.ai", "mistral-large-latest");

        await client.CompleteAsync("system-prompt", "user-message", null);

        Assert.NotNull(capturedRequest);
        Assert.Equal("https://custom.mistral.ai/v1/chat/completions", capturedRequest!.RequestUri?.ToString());
        Assert.Equal("Bearer my-api-key", capturedRequest.Headers.Authorization?.ToString());
    }
}