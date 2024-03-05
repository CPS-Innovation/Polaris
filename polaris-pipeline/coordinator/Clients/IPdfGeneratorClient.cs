using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Clients
{
    public interface IPdfGeneratorClient
    {
        Task<Stream> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType);
    }
}
