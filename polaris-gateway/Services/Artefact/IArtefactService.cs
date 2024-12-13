using Common.Domain.Ocr;
using Common.Domain.Pii;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public interface IArtefactService
{
    Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed);
    Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcrAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
    Task<ArtefactResult<(Guid?, IEnumerable<PiiLine>)>> GetPiiAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
}