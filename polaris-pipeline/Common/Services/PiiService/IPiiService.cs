using System.Collections.Generic;
using Common.Domain.Pii;
using Common.Domain.Ocr;
using System.Threading.Tasks;
using System;

namespace Common.Services.PiiService
{
    public interface IPiiService
    {
        Task<IEnumerable<PiiLine>> GetPiiResults(AnalyzeResults ocrResults, int caseId, string documentId, int characterLimit, Guid correlationId);
    }
}