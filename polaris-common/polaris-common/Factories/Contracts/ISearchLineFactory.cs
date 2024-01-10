using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using polaris_common.Domain.SearchIndex;
using polaris_common.ValueObjects;

namespace polaris_common.Factories.Contracts
{
    public interface ISearchLineFactory
    {
        SearchLine Create(long cmsCaseId, string cmsDocumentId, PolarisDocumentId polarisDocumentId, long versionId, string blobName, ReadResult readResult, Line line, int index);
    }
}