// <copyright file="ICookieService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Interface for handling cookie-related operations.
/// </summary>
public interface ICookieService
{
    /// <summary>
    /// Retrieves the case ID from the 'HSK' cookie.
    /// </summary>
    /// <param name="req">The HTTP request containing the cookies.</param>
    /// <returns>
    /// The case ID extracted from the 'HSK' cookie if present and valid; otherwise, <c>null</c>.
    /// </returns>
    string? GetCaseId(HttpRequest req);

    /// <summary>
    /// Retrieves the CMS cookies from the HTTP request.
    /// </summary>
    /// <param name="req">The HTTP request containing the cookies.</param>
    /// <returns>
    /// The CMS cookies if present; otherwise, <c>null</c>.
    /// </returns>
    string? GetCmsCookies(HttpRequest req);

    /// <summary>
    /// Retrieves the CMS token from the HTTP request.
    /// </summary>
    /// <param name="req">The HTTP request containing the cookies.</param>
    /// <returns>
    /// The CMS token if present; otherwise, <c>null</c>.
    /// </returns>
    string? GetCmsToken(HttpRequest req);

    /// <summary>
    /// Retrieves a specific part of the 'HSK' cookie by index.
    /// </summary>
    /// <param name="req">The HTTP request containing the cookies.</param>
    /// <param name="index">The index of the part to retrieve.</param>
    /// <returns>
    /// The requested part of the 'HSK' cookie if present and valid; otherwise, <c>null</c>.
    /// </returns>
    string? GetCookiePartByIndex(HttpRequest req, int index);

    /// <summary>
    /// Validates the cookies from the HTTP request.
    /// </summary>
    /// <param name="req">The HTTP request containing the cookies.</param>
    /// <returns>
    /// A tuple containing a boolean indicating if the cookies are valid, an error message if invalid, and the extracted case ID.
    /// </returns>
    (bool IsValid, string? ErrorMessage, int? CaseId) ValidateCookies(HttpRequest req);

    /// <summary>
    /// Validates the cookies from the HTTP request excluding the case id.
    /// </summary>
    /// <param name="req">The HTTP request containing the cookies.</param>
    /// <returns>
    /// A tuple containing a boolean indicating if the cookies are valid, an error message if invalid.
    /// </returns>
    (bool IsValid, string? ErrorMessage) ValidateCookiesWithNoCaseId(HttpRequest req);

    /// <summary>
    /// Appends a new value to the existing cookie with the specified name, or sets the cookie if it doesn't exist.
    /// </summary>
    /// <param name="req">The HTTP request containing the existing cookies, used to retrieve the current value of the cookie if it exists.</param>
    /// <param name="res">The HTTP response to which the cookie is added or updated.</param>
    /// <param name="name">The name of the cookie to append to or set.</param>
    /// <param name="value">The value to append to the cookie. If the cookie already exists, the value will be appended to the existing value.</param>
    /// <param name="options">The cookie options to set, including properties such as HttpOnly, Secure, and SameSite.</param>
    void AppendCookie(HttpRequest req, HttpResponse res, string name, string value, CookieOptions options);
}
