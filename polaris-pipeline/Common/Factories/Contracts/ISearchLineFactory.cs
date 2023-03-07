using Common.Domain.SearchIndex;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;

namespace Common.Factories.Contracts
{
	public interface ISearchLineFactory
	{
		SearchLine Create(Guid polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, ReadResult readResult, Line line, int index);
	}
}