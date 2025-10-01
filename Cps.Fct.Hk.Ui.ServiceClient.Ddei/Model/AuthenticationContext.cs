// <copyright file="AuthenticationContext.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The authentication context.
/// </summary>
public class AuthenticationContext
{
    private string? json;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationContext"/> class.
    /// </summary>
    /// <param name="cookies">The CMS classic authentication cookies.</param>
    /// <param name="token">The CMS modern authentication token.</param>
    /// <param name="expiryTime">The authentication expiry time.</param>
    public AuthenticationContext(
        string cookies,
        string token,
        DateTimeOffset expiryTime)
    {
        this.Cookies = cookies;
        this.Token = token;
        this.ExpiryTime = expiryTime;
    }

    /// <summary>
    /// Gets the CMS classic authentication cookies.
    /// </summary>
    [JsonPropertyName(nameof(AuthenticationContext.Cookies))]
    public string Cookies { get; init; }

    /// <summary>
    /// Gets the CMS modern authentication token.
    /// </summary>
    [JsonPropertyName(nameof(AuthenticationContext.Token))]
    public string Token { get; init; }

    /// <summary>
    /// Gets the authentication expiry time.
    /// </summary>
    [JsonPropertyName(nameof(AuthenticationContext.ExpiryTime))]
    public virtual DateTimeOffset ExpiryTime { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        this.json ??= JsonSerializer.Serialize(this);
        return this.json;
    }
}
