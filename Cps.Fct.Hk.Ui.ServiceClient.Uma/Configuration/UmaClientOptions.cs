// <copyright file="UmaClientOptions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Uma.Configuration;

/// <summary>
/// Represents the configuration options for the UMA client.
/// </summary>
public class UmaClientOptions
{

    /// <summary>
    /// The default section name in the appsettings.json file.
    /// </summary>
    public const string DefaultSectionName = $"UmaClient";

    /// <summary>
    /// 
    /// <summary>
    /// Gets or sets the base address of the Classification Service API.
    /// </summary>
    public string? BaseAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function key for authenticating requests to the Classification Service API.
    /// </summary>
    public string FunctionKey { get; set; } = string.Empty;
}
