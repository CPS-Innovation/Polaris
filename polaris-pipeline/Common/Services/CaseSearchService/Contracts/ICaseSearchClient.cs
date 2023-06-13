using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.Entity;
using Common.Domain.SearchIndex;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.CaseSearchService.Contracts
{
	public interface ICaseSearchClient
	{
		Task StoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);

        Task<IList<StreamlinedSearchLine>> Query(int caseId, List<BaseDocumentEntity> documents, string searchTerm, Guid correlationId);

        IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm, Guid correlationId);

        Task RemoveDocument(long caseId, string documentId, long versionId, Guid correlationId);

        Task RemoveResultsByBlobNameAsync(long cmsCaseId, string blobName, Guid correlationId);
	}
}

