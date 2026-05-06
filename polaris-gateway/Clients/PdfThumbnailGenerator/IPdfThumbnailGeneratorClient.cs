using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolarisGateway.Clients.PdfThumbnailGenerator;

public interface IPdfThumbnailGeneratorClient
{ 
    Task<HttpResponseMessage> GenerateThumbnailAsync(string caseUrn, int caseId, string materialId, int documentId, int maxDimensionPixel, int? pageIndex, string cmsAuthValues, Guid correlationId); 
    
    Task<HttpResponseMessage> GetThumbnailAsync(string caseUrn, int caseId, string materialId, int documentId, int maxDimensionPixel, int pageIndex, string cmsAuthValues, Guid correlationId);
}