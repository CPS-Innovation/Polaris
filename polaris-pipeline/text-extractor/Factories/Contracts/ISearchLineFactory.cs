using Common.Domain.SearchIndex;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Factories.Contracts
{
    public interface ISearchLineFactory
    {
        SearchLine Create(long cmsCaseId, string cmsDocumentId, string documentId, long versionId, string blobName, ReadResult readResult, Common.Domain.SearchIndex.Line line, int index);
    }
}