using System.Net;
using Common.Domain.SearchIndex;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Clients
{
    public interface IPipelineClient
    {
        Task<HttpStatusCode> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
        Task<TrackerDto> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
        Task<IActionResult> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
        Task<IActionResult> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
        Task CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
        Task<RedactPdfResponse> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId);
        Task<IList<StreamlinedSearchLine>> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);
    }
}
