using Common.Domain.Ocr;

namespace coordinator.Domain;

public class CompleteOcrResponse
{
    public bool BlobAlreadyExists { get; set; }

    public AnalyzeResultsStats OcrResult { get; set; }
}