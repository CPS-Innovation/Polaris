namespace PolarisGateway.Clients.PdfThumbnailGenerator;

public interface IPdfThumbnailGeneratorClient
{ 
    Task<HttpResponseMessage> GenerateThumbnailAsync(string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int? pageIndex, string cmsAuthValues, Guid correlationId); 
    
    Task<HttpResponseMessage> GetThumbnailAsync(string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int pageIndex, string cmsAuthValues, Guid correlationId);
}