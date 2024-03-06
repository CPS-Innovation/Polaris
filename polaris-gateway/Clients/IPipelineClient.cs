using System.Net;
using Common.Domain.SearchIndex;
using Common.Dto.Case;
using Common.Dto.Request;
using Common.Dto.Tracker;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace PolarisGateway.Clients
{
    public interface IPipelineClient
    {
        Task<IList<CaseDto>> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId);
        Task<CaseDto> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
        Task<HttpStatusCode> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
        Task<TrackerDto> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
        Task DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
        Task<Stream> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
        Task<IActionResult> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
        Task CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
        Task SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId);
        Task<IList<StreamlinedSearchLine>> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);
    }
}
