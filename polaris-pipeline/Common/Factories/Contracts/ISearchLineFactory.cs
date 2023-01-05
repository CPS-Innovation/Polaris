using Common.Domain.SearchIndex;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Factories.Contracts
{
	public interface ISearchLineFactory
	{
		SearchLine Create(long caseId, string documentId, long versionId, string blobName, ReadResult readResult, Line line, int index);
	}
}