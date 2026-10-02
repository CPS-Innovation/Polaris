// <copyright file="RedactAndLogService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Services;

using Common.Dto.Request;
using Common.Dto.Response;
using Microsoft.DurableTask.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RedactAndLogService : IRedactAndLogService
{
    public Task<RedactAndLogResponse> StartOrReuseAsync(
                                int caseId,
                                string materialId,
                                long documentId,
                                RedactAndLogRequestDto request,
                                CmsAuthValues cmsAuthValues,
                                Guid correlationId,
                                DurableTaskClient orchestrationClient,
                                CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    private static string CreateInstanceId(
    Guid correlationId,
    int caseId,
    string materialId,
    long documentId)
    {
        return $"RedactAndLog-{correlationId:N}-{caseId}-{materialId}-{documentId}";
    }
}
