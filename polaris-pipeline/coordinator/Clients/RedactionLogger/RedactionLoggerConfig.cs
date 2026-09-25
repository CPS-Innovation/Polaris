// <copyright file="RedactionLoggerConfig.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.RedactionLogger;

using System.ComponentModel.DataAnnotations;

public class RedactionLoggerConfig
{
    public const string DefaultSectionName = "RedactionLogger";

    [Required]
    public string Scope { get; init; } = string.Empty;

    [Required]
    public string TenantId { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;
}
