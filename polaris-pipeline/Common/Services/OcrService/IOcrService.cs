using System;
using System.IO;
using System.Threading.Tasks;
using Common.Services.OcrService.Domain;

namespace Common.Services.OcrService
{
    public interface IOcrService
    {
        Task<Guid> InitiateOperationAsync(Stream stream, Guid correlationId);

        Task<OcrOperationResult> GetOperationResultsAsync(Guid operationId, Guid correlationId);
    }
}
