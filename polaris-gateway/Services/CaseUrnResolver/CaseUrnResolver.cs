// <copyright file="CaseUrnResolver.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.CaseUrnResolver;

using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using DdeiClient.Clients.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CaseUrnResolver(IMemoryCache memoryCache, IMasterDataServiceClient mdsClient, ILogger<CaseUrnResolver> logger)
    : ICaseUrnResolver
{
    public async Task<string> ResolveCaseUrnAsync(int caseId, CmsAuthValues cmsAuthValues, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(caseId);
        if (memoryCache.TryGetValue(cacheKey, out string cachedUrn) && !string.IsNullOrWhiteSpace(cachedUrn))
        {
            return cachedUrn;
        }

        var caseSummary = await mdsClient.GetCaseSummaryAsync(new GetCaseSummaryRequest(caseId, cmsAuthValues.CorrelationId), cmsAuthValues, cancellationToken);

        var resolvedUrn = caseSummary?.Urn;
        if (string.IsNullOrWhiteSpace(resolvedUrn))
        {
            throw new InvalidOperationException($"Unable to resolve URN for caseId={caseId} from GetCaseSummary.");
        }

        this.Cache(caseId, resolvedUrn);
        logger.LogInformation("Resolved case URN {CaseUrn} for caseId {CaseId}", resolvedUrn, caseId);

        return resolvedUrn;
    }

    private static string GetCacheKey(int caseId) => $"case-urn:{caseId}";

    private void Cache(int caseId, string urn)
    {
        memoryCache.Set(GetCacheKey(caseId), urn, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
        });
    }
}
