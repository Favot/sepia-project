using Common.Extraction;
using Common.Llm;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UploadsController(IMistralClient client, ILogger<UploadsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<object>> Upload([FromForm] IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest("No file provided");

        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");
        await using (var stream = System.IO.File.Create(tempPath))
            await file.CopyToAsync(stream);

        logger.LogInformation("Stored temp file {FileName} at {Path}", file.FileName, tempPath);

        var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
        System.IO.File.Delete(tempPath);

        var (fields, ocrText, extractionMethod) = await FieldExtractor.ExtractFromDocumentAsync(
            fileBytes, file.FileName, client, logger);

        logger.LogInformation("OCR + extraction completed for {FileName}: SIRET={Siret}, Engagement={Engagement}, CodeService={CodeService}",
            file.FileName, fields.NumeroSiret, fields.NumeroEngagement, fields.CodeService);

        return Ok(new
        {
            fileName = file.FileName,
            numeroSiret = fields.NumeroSiret,
            numeroEngagement = fields.NumeroEngagement,
            codeService = fields.CodeService,
            extractionMethod,
            rawOcr = ocrText
        });
    }
}
