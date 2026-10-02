// <copyright file="RedactionLoggerConfig.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.RedactionLogger;

using System.ComponentModel.DataAnnotations;

public class RedactionLoggerConfig : IHttpClientConfig
{
    public const string DefaultSectionName = "RedactionLogger";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TimeoutSeconds { get; init; } = 200;

    [Range(0, int.MaxValue)]
    public int MaxRetries { get; init; } = 5;

    [Required]
    public string Scope { get; init; } = string.Empty;

    [Required]
    public string TenantId { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;
    [Required]
    public string ClientSecret { get; init; } = string.Empty;
}
