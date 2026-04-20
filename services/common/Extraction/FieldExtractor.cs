namespace Common.Extraction;

using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Llm;
using Microsoft.Extensions.Logging;

public class ExtractedFields
{
    public string? NumeroEngagement { get; set; }
    public string? NumeroSiret { get; set; }
    public string? CodeService { get; set; }
}

public class LlmExtractionResult
{
    [JsonPropertyName("numero_siret")]
    public string? NumeroSiret { get; set; }

    [JsonPropertyName("numero_engagement")]
    public string? NumeroEngagement { get; set; }

    [JsonPropertyName("code_service")]
    public string? CodeService { get; set; }

    public ExtractedFields ToExtractedFields() => new()
    {
        NumeroSiret = NumeroSiret,
        NumeroEngagement = NumeroEngagement,
        CodeService = CodeService
    };
}

public static class FieldExtractor
{
    private const string SystemPrompt = "Tu es un assistant spécialisé dans l'extraction de champs de factures et de documents fiscaux français. Extrais les champs suivants du texte OCR fourni : numero_siret (Numéro de SIRET de la société), numero_engagement (Numéro d'engagement budgétaire), et code_service (Code de service). Retourne uniquement le JSON structuré demandé, sans texte supplémentaire.";

    public static object BuildDocumentAnnotationSchema()
    {
        return new
        {
            type = "json_schema",
            json_schema = new
            {
                name = "invoice_fields",
                strict = true,
                schema = new
                {
                    properties = new
                    {
                        numero_siret = new { description = "Numéro de SIRET de la société", type = "string" },
                        numero_engagement = new { description = "Numéro d'engagement budgétaire", type = "string" },
                        code_service = new { description = "Code de service", type = "string" }
                    },
                    required = new[] { "numero_siret", "numero_engagement" }
                }
            }
        };
    }

    public static ExtractedFields Extract(string markdown)
    {
        var siret = ExtractSiret(markdown);
        var engagement = ExtractEngagement(markdown);
        var codeService = ExtractCodeService(markdown);
        return new ExtractedFields { NumeroSiret = siret, NumeroEngagement = engagement, CodeService = codeService };
    }

    public static async Task<(ExtractedFields Fields, string ExtractionMethod)> ExtractWithFallbackAsync(
        string markdown,
        IMistralClient client,
        ILogger? logger = null)
    {
        try
        {
            var fields = await ExtractWithLlmAsync(markdown, client);
            logger?.LogInformation("LLM extraction succeeded. SIRET={Siret}, Engagement={Engagement}, CodeService={CodeService}",
                fields.NumeroSiret, fields.NumeroEngagement, fields.CodeService);
            return (fields, "llm");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "LLM extraction failed, falling back to regex. MarkdownLength={Length}", markdown.Length);
            return (Extract(markdown), "regex");
        }
    }

    public static async Task<(ExtractedFields Fields, string OcrText, string ExtractionMethod)> ExtractFromDocumentAsync(
        byte[] fileBytes,
        string fileName,
        IMistralClient client,
        ILogger? logger = null)
    {
        var ocrText = await client.ExtractOcrAsync(fileBytes, fileName);
        var (fields, extractionMethod) = await ExtractWithFallbackAsync(ocrText, client, logger);
        return (fields, ocrText, extractionMethod);
    }

    public static async Task<ExtractedFields> ExtractWithLlmAsync(string markdown, IMistralClient client)
    {
        var responseJson = await client.CompleteAsync(SystemPrompt, markdown, BuildDocumentAnnotationSchema());
        var llmResult = JsonSerializer.Deserialize<LlmExtractionResult>(responseJson)
            ?? throw new InvalidOperationException("Failed to deserialize LLM response");
        return llmResult.ToExtractedFields();
    }

    private static string? ExtractSiret(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            text, @"Numéro\s+de\s+SIRET\s*[:\s]+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ExtractEngagement(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            text, @"Numéro\s+Engagement\s*[:\s]+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ExtractCodeService(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            text, @"(?:Code\s+(?:de\s+)?Service|Service)\s*[:\s]+(\S+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }
}
