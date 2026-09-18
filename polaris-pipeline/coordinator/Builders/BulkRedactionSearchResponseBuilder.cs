// <copyright file="BulkRedactionSearchResponseBuilder.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Builders;

using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using coordinator.Domain;
using coordinator.Enums;
using System.Collections.Generic;

public class BulkRedactionSearchResponseBuilder : IBulkRedactionSearchResponseBuilder
{
    private readonly BulkRedactionSearchResponse bulkRedactionSearchResponse = new ();

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshInitiated()
    {
        this.bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatus.Initiated;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshProcessing()
    {
        this.bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatus.Processing;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshCompleted()
    {
        this.bulkRedactionSearchResponse.DocumentRefreshStatus = OrchestrationProviderStatus.Completed;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildDocumentRefreshFailed(string failedReason, bool isNotFound = false)
    {
        this.bulkRedactionSearchResponse.DocumentRefreshStatus = isNotFound ? OrchestrationProviderStatus.NotStarted : OrchestrationProviderStatus.Failed;
        this.bulkRedactionSearchResponse.FailedReason = failedReason;
        this.bulkRedactionSearchResponse.IsNotFound = isNotFound;
        return this;
    }

    public IBulkRedactionSearchResponseBuilder BuildRedactionDefinitions(IEnumerable<RedactionDefinitionDto> redactionDefinitionDtos)
    {
        this.bulkRedactionSearchResponse.RedactionDefinitions = redactionDefinitionDtos;
        return this;
    }

    public BulkRedactionSearchResponse Build(BulkRedactionSearchDto bulkRedactionSearchDto)
    {
        this.bulkRedactionSearchResponse.Urn = bulkRedactionSearchDto.Urn;
        this.bulkRedactionSearchResponse.CaseId = bulkRedactionSearchDto.CaseId;
        this.bulkRedactionSearchResponse.MaterialId = bulkRedactionSearchDto.MaterialId;
        this.bulkRedactionSearchResponse.DocumentId = bulkRedactionSearchDto.DocumentId;
        this.bulkRedactionSearchResponse.SearchText = bulkRedactionSearchDto.SearchText;
        return this.bulkRedactionSearchResponse;
    }
}
