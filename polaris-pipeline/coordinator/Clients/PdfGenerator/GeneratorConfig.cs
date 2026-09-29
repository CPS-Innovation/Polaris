// <copyright file="GeneratorConfig.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.PdfGenerator;

using System.ComponentModel.DataAnnotations;
public class GeneratorConfig : IHttpClientConfig
{
    public const string DefaultSectionName = "Generator";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TimeoutSeconds { get; init; } = 200;

    [Range(0, int.MaxValue)]
    public int MaxRetries { get; init; } = 5;
}
