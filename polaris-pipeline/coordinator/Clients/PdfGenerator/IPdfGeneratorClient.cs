using Common.Domain.Document;
using coordinator.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Clients.PdfGenerator
{
    public interface IPdfGeneratorClient
    {
        Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType);
    }
}
