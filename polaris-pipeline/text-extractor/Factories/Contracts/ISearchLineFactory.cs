using Common.Domain.SearchIndex;
using Common.ValueObjects;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Factories.Contracts
{
    public interface ISearchLineFactory
    {
        SearchLine Create(long cmsCaseId, string cmsDocumentId, PolarisDocumentId polarisDocumentId, long versionId, string blobName, ReadResult readResult, Line line, int index);
    }
}