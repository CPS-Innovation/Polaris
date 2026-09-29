// <copyright file="PdfRedactorConfig.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.PdfRedactor;

using System.ComponentModel.DataAnnotations;

public class PdfRedactorConfig : IHttpClientConfig
{
    public const string DefaultSectionName = "Redactor";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string AccessKey { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TimeoutSeconds { get; init; } = 200;

    [Range(0, int.MaxValue)]
    public int MaxRetries { get; init; } = 5;
}
