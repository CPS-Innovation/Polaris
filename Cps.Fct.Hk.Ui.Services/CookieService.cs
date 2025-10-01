// <copyright file="CookieService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for handling cookie-related operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CookieService"/> class.
/// </remarks>
/// <param name="logger">The logger instance for logging information.</param>
public class CookieService(ILogger<CookieService> logger)
    : ICookieService
{
    private readonly ILogger<CookieService> logger = logger;

    /// <inheritdoc />
    public string? GetCaseId(HttpRequest req)
    {
        return this.GetCookiePartByIndex(req, 0);
    }

    /// <inheritdoc />
    public string? GetCmsCookies(HttpRequest req)
    {
        return this.GetCookiePartByIndex(req, 1);
    }

    /// <inheritdoc />
    public string? GetCmsToken(HttpRequest req)
    {
        return this.GetCookiePartByIndex(req, 2);
    }

    /// <inheritdoc />
    public string? GetCookiePartByIndex(HttpRequest req, int index)
    {
        if (req.Cookies.TryGetValue("HSK", out string? hskCookie))
        {
            string[] hskParts = hskCookie.Split(':');

            if (hskParts.Length > index)
            {
                return hskParts[index];
            }
            else
            {
                this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} HSK cookie does not contain enough parts. Requested index: {index}.");
                return null;
            }
        }
        else
        {
            this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} HSK cookie not found in the request.");
            return null;
        }
    }

    /// <inheritdoc />
    public (bool IsValid, string? ErrorMessage, int? CaseId) ValidateCookies(HttpRequest req)
    {
        string? caseIdString = this.GetCaseId(req);
        string? cmsCookiesString = this.GetCmsCookies(req);
        string? cmsTokenString = this.GetCmsToken(req);

        if (string.IsNullOrEmpty(caseIdString) || !int.TryParse(caseIdString, out int caseId))
        {
            return (false, "Invalid or missing case_id in the HSK cookie.", null);
        }

        if (string.IsNullOrEmpty(cmsCookiesString))
        {
            return (false, "Invalid or missing cmsCookies in the HSK cookie.", null);
        }

        if (string.IsNullOrEmpty(cmsTokenString))
        {
            return (false, "Invalid or missing cmsToken in the HSK cookie.", null);
        }

        return (true, null, caseId);
    }

    /// <inheritdoc />
    public (bool IsValid, string? ErrorMessage) ValidateCookiesWithNoCaseId(HttpRequest req)
    {
        string? cmsCookiesString = this.GetCmsCookies(req);
        string? cmsTokenString = this.GetCmsToken(req);

        if (string.IsNullOrEmpty(cmsCookiesString))
        {
            return (false, "Invalid or missing cmsCookies in the HSK cookie.");
        }

        if (string.IsNullOrEmpty(cmsTokenString))
        {
            return (false, "Invalid or missing cmsToken in the HSK cookie.");
        }

        return (true, null);
    }

    /// <inheritdoc />
    public void AppendCookie(HttpRequest req, HttpResponse res, string name, string value, CookieOptions options)
    {
        // Get the existing cookie value if it exists
        string? existingValue = req.Cookies[name];

        // If the cookie already exists, append the new value with a colon separator, otherwise use the new value directly
        string newValue = existingValue != null
            ? $"{existingValue}:{value}" // Simply append with a colon, without encoding it
            : value;  // If no existing value, set the new value directly

        // Set the cookie with the updated value
        res.Cookies.Append(name, newValue, options);
    }
}
