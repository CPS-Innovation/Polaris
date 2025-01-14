using Common.Domain.Ocr;
using coordinator.Durable.Orchestration;

namespace coordinator.Domain;

public class OcrOperationPollingResult
{
    public bool BlobAlreadyExists { get; set; }

    public PollingResult<AnalyzeResultsStats> OcrPollingResult { get; set; }
}