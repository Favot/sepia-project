namespace Common.Contracts.DTOs;

public record UploadResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? OcrText,
    DateTime CreatedAt
);
