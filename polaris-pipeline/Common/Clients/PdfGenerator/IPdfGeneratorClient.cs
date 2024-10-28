using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Domain.Document;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Clients.PdfGenerator
{
    public interface IPdfGeneratorClient
    {
        Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string caseUrn, int caseId, string documentId, long versionId, Stream documentStream, FileType fileType);
    }
}
