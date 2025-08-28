// <copyright file="BaseFunction.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using System;
using System.Net;
using Common.Constants;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PolarisGateway.Functions;

public class BaseFunction(ILogger? logger = null)
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

    protected string GetCmsToken(HttpRequest req)
    {
        return this.GetCookiePartByIndex(req, 0);
    }

    protected string GetCmsCookie(HttpRequest req, bool urlEncode = false)
    {
        string cookie = this.GetCookiePartByIndex(req, 4);

        if (urlEncode)
        {
            return WebUtility.UrlEncode(cookie);
        }

        return cookie;
    }

    protected CmsAuthValues BuildCmsAuthValues(HttpRequest req)
    {
        var token = this.GetCmsToken(req) ?? string.Empty;
        var cookie = this.GetCmsCookie(req) ?? string.Empty;
        var correlation = EstablishCorrelation(req);

        return new CmsAuthValues(cookie, token, correlation);
    }

    private string? GetCookiePartByIndex(HttpRequest req, int index)
    {
        if (req.Cookies.TryGetValue(HttpHeaderKeys.CmsAuthValues, out string? cmsCookie))
        {
            string[] cmsParts = cmsCookie.Split(',');

            if (cmsParts.Length > index)
            {
                return cmsParts[index].Split(':')[1].Replace("\"", ""); ;
            }
            else
            {
                this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} CMS cookie does not contain enough parts. Requested index: {index}.");
                return null;
            }
        }
        else
        {
            this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} CMS cookie not found in the request.");
            return null;
        }
    }
}