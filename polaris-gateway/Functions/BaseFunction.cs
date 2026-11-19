// <copyright file="BaseFunction.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Constants;
using Common.Dto.Request;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.TelemetryEvents;
using System;

public class BaseFunction(ILogger logger = null)
{
    private readonly ILogger logger = logger;

    /// <summary>
    /// Raises <see cref="CmsPrefixUsageEvent"/> telemetry when a non-legacy endpoint receives a
    /// materialId using the temporarily-supported "CMS-" prefix, rather than a bare numeric id.
    /// Used to gate the Stage 3 removal of CMS- support on real usage data.
    /// </summary>
    protected static void TrackCmsPrefixUsage(ILogger logger, string rawMaterialId, int caseId, string documentId, string operationName)
    {
        if (logger is null || rawMaterialId is null || !rawMaterialId.StartsWith("CMS-", StringComparison.Ordinal))
        {
            return;
        }

        logger.TrackEvent(new CmsPrefixUsageEvent(caseId, documentId, operationName));
    }

    protected static Guid EstablishCorrelation(HttpRequest req) =>
        req.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationId) &&
        Guid.TryParse(correlationId, out var parsedCorrelationId) ?
            parsedCorrelationId :
            Guid.NewGuid();

    protected static string EstablishCmsAuthValues(HttpRequest req)
    {
        req.Cookies.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValues);
        return cmsAuthValues;
    }

    protected CmsAuthValues BuildCmsAuthValues(HttpRequest req)
    {
        if (req?.Cookies is null)
        {
            return null;
        }

        var cmsAuthValues = EstablishCmsAuthValues(req);
        var correlation = EstablishCorrelation(req);

        return new CmsAuthValues(cmsAuthValues, correlation);
    }
}
