using System;
using System.IO;
using System.Threading.Tasks;
using coordinator.Services.OcrService.Domain;

namespace coordinator.Services.OcrService
{
    public interface IOcrService
    {
        Task<Guid> InitiateOperationAsync(Stream stream, Guid correlationId);

        Task<(bool, AnalyzeResults)> GetOperationResultsAsync(Guid operationId, Guid correlationId);
    }
}
