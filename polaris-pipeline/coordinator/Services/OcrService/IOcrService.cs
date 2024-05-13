using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace coordinator.Services.OcrService
{
    public interface IOcrService
    {
        [Obsolete("Use GetOcrResultsInitiateAsync instead")]
        Task<AnalyzeResults> GetOcrResultsAsync(Stream stream, Guid correlationId);

        Task<Guid> InitiateOperationAsync(Stream stream, Guid correlationId);

        Task<(bool, ReadOperationResult)> GetOperationResultsAsync(Guid operationId, Guid correlationId);
    }
}
