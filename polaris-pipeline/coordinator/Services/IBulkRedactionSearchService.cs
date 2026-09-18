// <copyright file="IBulkRedactionSearchService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Services;

using Common.Dto.Request;
using coordinator.Domain;
using Microsoft.DurableTask.Client;
using System.Threading;
using System.Threading.Tasks;
using coordinator.Enums;

public interface IBulkRedactionSearchService
{
    Task<BulkRedactionSearchResponse> InitiateOrOrchestrateOcr(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken);

    Task<BulkRedactionSearchResponse> GetOcrSearchResults(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken);
}
