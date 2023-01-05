using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.SearchIndexService.Contracts
{
	public interface ISearchIndexService
	{
		Task StoreResultsAsync(AnalyzeResults analyzeResults, long caseId, string documentId, long versionId, string blobName, Guid correlationId);

		Task RemoveResultsByBlobNameAsync(long caseId, string blobName, Guid correlationId);
	}
}

