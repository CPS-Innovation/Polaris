using System;
using System.Threading.Tasks;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.SearchIndexService.Contracts
{
	public interface ISearchIndexService
	{
		Task SendStoreResultsAsync(AnalyzeResults analyzeResults, PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);

		Task<bool> WaitForStoreResultsAsync(AnalyzeResults analyzeResults, long cmsCaseId, string cmsDocumentId, long versionId, Guid correlationId);

        Task RemoveResultsByBlobNameAsync(long cmsCaseId, string blobName, Guid correlationId);
	}
}

