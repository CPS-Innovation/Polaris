using System;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.Ocr;

namespace Common.Services.OcrService
{
    public interface IOcrService
    {
        Task<Guid> InitiateOperationAsync(Stream stream, Guid correlationId);

        Task<OcrOperationResult> GetOperationResultsAsync(Guid operationId, Guid correlationId);
    }
}
