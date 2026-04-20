namespace Common.Llm;

public interface IMistralClient
{
    Task<string> CompleteAsync(string systemPrompt, string userMessage, object? responseFormat = null);
    Task<string> ExtractOcrAsync(byte[] fileBytes, string fileName);
}
