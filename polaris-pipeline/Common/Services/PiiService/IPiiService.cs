using System.Collections.Generic;
using Common.Domain.Pii;
using Common.Domain.Ocr;
using System.Threading.Tasks;
using System;

namespace Common.Services.PiiService
{
    public interface IPiiService
    {
        Task<IEnumerable<PiiLine>> GetPiiResultsAsync(AnalyzeResults ocrResults, int caseId, string documentId, Guid correlationId);
    }
}