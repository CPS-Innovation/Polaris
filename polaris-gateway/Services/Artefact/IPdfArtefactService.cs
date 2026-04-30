using PolarisGateway.Services.Artefact.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public interface IPdfArtefactService
{
    public Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId, bool isOcrProcessed, bool forceRefresh = false);
}