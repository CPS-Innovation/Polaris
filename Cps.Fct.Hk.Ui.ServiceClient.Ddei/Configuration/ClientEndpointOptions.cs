// <copyright file="ClientEndpointOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Configuration;

using System;
using System.Collections.Generic;

/// <summary>
/// The options for a client endpoint.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ClientEndpointOptions"/> class.
/// </remarks>
/// <param name="baseAddress">The service base address.</param>
/// <param name="credentialName">The credential name.</param>
/// <param name="credentialPassword">The credential password.</param>
/// <param name="functionKey">The function key required to call an Azure Function.</param>
/// <param name="relativePath">The relative paths supported by the service.</param>
/// <param name="timeout">The timeout for the HTTP client.</param>
public class ClientEndpointOptions(
    Uri baseAddress,
    string? credentialName,
    string? credentialPassword,
    string? functionKey,
    IReadOnlyDictionary<string, string> relativePath,
    TimeSpan timeout)
{
    /// <summary>
    /// The default section name in the appsettings.json file.
    /// </summary>
    public const string DefaultSectionName = $"DDEIClient";

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientEndpointOptions"/> class.
    /// </summary>
    public ClientEndpointOptions()
        : this(
              baseAddress: new Uri("#", UriKind.Relative),
              credentialName: null,
              credentialPassword: null,
              functionKey: null,
              relativePath: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
              timeout: TimeSpan.FromSeconds(100))
    {
    }

    /// <summary>
    /// Gets the service base address.
    /// </summary>
    public Uri BaseAddress { get; init; } = baseAddress;

    /// <summary>
    /// Gets the credential name.
    /// </summary>
    public string? CredentialName { get; init; } = credentialName;

    /// <summary>
    /// Gets the credential password.
    /// </summary>
    public string? CredentialPassword { get; init; } = credentialPassword;

    /// <summary>
    /// Gets the function key required to call an Azure Function.
    /// </summary>
    public string? FunctionKey { get; init; } = functionKey;

    /// <summary>
    /// Gets the relative paths supported by the service.
    /// </summary>
    public IReadOnlyDictionary<string, string> RelativePath { get; init; } = relativePath;

    /// <summary>
    /// Gets the timeout for the HTTP client.
    /// </summary>
    public TimeSpan Timeout { get; init; } = timeout;
}
