using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using coordinator.Domain;
using coordinator.Enums;
using System.Collections.Generic;

namespace coordinator.Builders;

public class BulkRedactionSearchResponseBuilder : IBulkRedactionSearchResponseBuilder
{
    private readonly BulkRedactionSearchResponse _bulkRedactionSearchResponse = new();

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshInitiated()
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatus.Initiated;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshProcessing()
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatus.Processing;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshCompleted()
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatus.Completed;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshFailed(string failedReason, bool isNotFound = false)
    {
        _bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatus.Failed;
        _bulkRedactionSearchResponse.FailedReason = failedReason;
        _bulkRedactionSearchResponse.IsNotFound = isNotFound;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildRedactionDefinitions(IEnumerable<RedactionDefinitionDto> redactionDefinitionDtos)
    {
        _bulkRedactionSearchResponse.RedactionDefinitions = redactionDefinitionDtos;
        return this;
    }

    public BulkRedactionSearchResponse Build(BulkRedactionSearchDto bulkRedactionSearchDto)
    {
        _bulkRedactionSearchResponse.Urn = bulkRedactionSearchDto.Urn;
        _bulkRedactionSearchResponse.CaseId = bulkRedactionSearchDto.CaseId;
        _bulkRedactionSearchResponse.DocumentId = bulkRedactionSearchDto.DocumentId;
        _bulkRedactionSearchResponse.VersionId = bulkRedactionSearchDto.VersionId;
        _bulkRedactionSearchResponse.SearchText = bulkRedactionSearchDto.SearchText;
        return _bulkRedactionSearchResponse;
    }
}