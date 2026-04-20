namespace Common.Llm;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class MistralClient : IMistralClient
{
    private readonly HttpClient _httpClient;
    public string ApiKey { get; }
    public string BaseUrl { get; }
    public string ChatModel { get; }
    public string OcrModel { get; }

    public MistralClient(HttpClient httpClient, string apiKey, string baseUrl, string chatModel, string ocrModel = "mistral-ocr-latest")
    {
        _httpClient = httpClient;
        ApiKey = apiKey;
        BaseUrl = baseUrl;
        ChatModel = chatModel;
        OcrModel = ocrModel;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userMessage, object? responseFormat = null)
    {
        var messages = new object[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userMessage }
        };

        object requestBody;
        if (responseFormat != null)
        {
            requestBody = new { model = ChatModel, messages, response_format = responseFormat };
        }
        else
        {
            requestBody = new { model = ChatModel, messages };
        }

        using var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/chat/completions")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Mistral Chat API failed with StatusCode {(int)response.StatusCode}: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        var choices = doc.RootElement.GetProperty("choices");
        var message = choices[0].GetProperty("message");

        string resultJson;
        if (message.TryGetProperty("tool_calls", out var toolCalls) &&
            toolCalls.ValueKind == JsonValueKind.Array &&
            toolCalls.GetArrayLength() > 0)
        {
            resultJson = toolCalls[0].GetProperty("function").GetProperty("arguments").GetString() ?? "{}";
        }
        else if (message.TryGetProperty("content", out var msgContent) &&
                 msgContent.ValueKind == JsonValueKind.String)
        {
            resultJson = msgContent.GetString() ?? "{}";
        }
        else
        {
            throw new InvalidOperationException($"Unexpected Mistral response structure: message={message.GetRawText()}");
        }

        return resultJson;
    }

    public async Task<string> ExtractOcrAsync(byte[] fileBytes, string fileName)
    {
        var base64File = Convert.ToBase64String(fileBytes);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var mimeType = extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/pdf"
        };

        var requestBody = new
        {
            model = OcrModel,
            document = new
            {
                type = "document_url",
                document_url = $"data:{mimeType};base64,{base64File}"
            }
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/ocr")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Mistral OCR API failed with StatusCode {(int)response.StatusCode}: {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);

        return string.Join("\n", doc.RootElement
            .GetProperty("pages")
            .EnumerateArray()
            .Select(p => p.GetProperty("markdown").GetString()));
    }
}
