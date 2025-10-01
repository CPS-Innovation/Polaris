// <copyright file="AuthenticationHeader.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The authentication header.
/// </summary>
public class AuthenticationHeader : AuthenticationContext
{
    /// <summary>
    /// The CMS authentication header name.
    /// </summary>
    public const string AuthenticationHeaderName = "Cms-Auth-Values";

    private string? json;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationHeader"/> class.
    /// </summary>
    /// <param name="cookies">The CMS classic authentication cookies.</param>
    /// <param name="token">The CMS modern authentication token.</param>
    /// <param name="expiryTime">The authentication expiry time.</param>
    public AuthenticationHeader(
        string cookies,
        string token,
        DateTimeOffset expiryTime)
            : base(cookies, token, expiryTime)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationHeader"/> class.
    /// </summary>
    /// <param name="authenticationResponse">The authentication response.</param>
    public AuthenticationHeader(AuthenticationContext authenticationResponse)
            : base(authenticationResponse.Cookies, authenticationResponse.Token, authenticationResponse.ExpiryTime)
    {
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public override DateTimeOffset ExpiryTime { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        this.json ??= JsonSerializer.Serialize(this);
        return this.json;
    }
}
