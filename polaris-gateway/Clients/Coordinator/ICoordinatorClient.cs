// <copyright file="ICoordinatorClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Clients.Coordinator;

using Common.Dto.Request;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public interface ICoordinatorClient
{
    Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId, bool isLegacy = true);

    Task<HttpResponseMessage> GetTrackerGetCaseAsync(string caseUrn, int caseId, Guid correlationId, bool isLegacy = true);

    Task<HttpResponseMessage> GetTrackerBulkRedactionSearchAsync(string caseUrn, int caseId, string materialId, long documentId, string searchText, Guid correlationId, CancellationToken cancellationToken = default, bool isLegacy = true);

    Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId, bool isLegacy = true);

    Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, string materialId, long documentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId, bool isLegacy = true);

    Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId, bool isLegacy = true);

    Task<HttpResponseMessage> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId, bool isLegacy = true);

    Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, string materialId, long documentId, ModifyDocumentDto modifyDocumentDto, string cmsAuthValues, Guid correlationId, bool isLegacy = true);

    Task<HttpResponseMessage> BulkRedactionInitiateSearchAsync(int caseId, string materialId, long documentId, Guid correlationId, string cmsAuthValues, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> BulkRedactionRetrieveSearchResultsAsync(int caseId, string materialId, long documentId, string searchText, Guid correlationId, string cmsAuthValues, CancellationToken cancellationToken = default);
}
