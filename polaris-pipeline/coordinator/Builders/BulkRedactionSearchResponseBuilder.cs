using System.Collections.Generic;
using Common.Dto.Request.Redaction;
using coordinator.Domain;
using coordinator.Enums;

namespace coordinator.Builders;

public class BulkRedactionSearchResponseBuilder : IBulkRedactionSearchResponseBuilder
{
    private BulkRedactionSearchResponse _bulkRedactionSearchResponse = new();

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshInitiated()
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatuses.Initiated;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshProcessing()
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatuses.Processing;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshCompleted()
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatuses.Completed;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshFailed(string failedReason, bool isNotFound = false)
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatuses.Failed;
        _bulkRedactionSearchResponse.FailedReason = failedReason;
        _bulkRedactionSearchResponse.IsNotFound = isNotFound;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildRedactionDefinitions(IEnumerable<RedactionDefinitionDto> redactionDefinitionDtos)
    {
        _bulkRedactionSearchResponse.RedactionDefinitions = redactionDefinitionDtos;
        return this;
    }

    public BulkRedactionSearchResponse Build(string urn, int caseId, string documentId, long versionId, string searchText)
    {
        _bulkRedactionSearchResponse.Urn = urn;
        _bulkRedactionSearchResponse.CaseId = caseId;
        _bulkRedactionSearchResponse.DocumentId = documentId;
        _bulkRedactionSearchResponse.VersionId = versionId;
        _bulkRedactionSearchResponse.SearchText = searchText;
        return _bulkRedactionSearchResponse;
    }
}