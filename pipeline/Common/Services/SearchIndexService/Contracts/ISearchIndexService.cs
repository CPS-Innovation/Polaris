using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.SearchIndexService.Contracts
{
	public interface ISearchIndexService
	{
		Task StoreResultsAsync(AnalyzeResults analyzeresults, long caseId, string documentId, long versionId, string blobName, Guid correlationId);

		Task RemoveResultsByDocumentVersionAsync(long caseId, string documentId, long versionId, Guid correlationId);
		
		Task RemoveResultsByBlobNameAsync(long caseId, string blobName, Guid correlationId);
	}
}

