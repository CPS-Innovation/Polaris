using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using coordinator.Domain;
using System.Collections.Generic;

namespace coordinator.Builders;

public interface IBulkRedactionSearchResponseBuilder
{
    IBulkRedactionSearchResponseBuilder BuildDocumentRefreshInitiated();

    IBulkRedactionSearchResponseBuilder BuildDocumentRefreshProcessing();

    IBulkRedactionSearchResponseBuilder BuildDocumentRefreshCompleted();

    IBulkRedactionSearchResponseBuilder BuildDocumentRefreshFailed(string failedReason, bool isNotFound = false);

    IBulkRedactionSearchResponseBuilder BuildRedactionDefinitions(IEnumerable<RedactionDefinitionDto> redactionDefinitionDtos);

    BulkRedactionSearchResponse Build(BulkRedactionSearchDto bulkRedactionSearchDto);
}