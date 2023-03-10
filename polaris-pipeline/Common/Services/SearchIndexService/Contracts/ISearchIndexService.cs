using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.SearchIndexService.Contracts
{
	public interface ISearchIndexService
	{
		Task StoreResultsAsync(AnalyzeResults analyzeResults, Guid polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);

		Task RemoveResultsByBlobNameAsync(long cmsCaseId, string blobName, Guid correlationId);
	}
}

