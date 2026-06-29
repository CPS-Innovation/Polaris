// <copyright file="BaseFunction.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Constants;
using Common.Dto.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

public class BaseFunction(ILogger logger = null)
{
    private readonly ILogger logger = logger;

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

    protected static CmsAuthValues BuildCmsAuthValues(HttpRequest req)
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
