// <copyright file="TextExtractorConfig.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.TextExtractor;

using System.ComponentModel.DataAnnotations;

public class TextExtractorConfig : IHttpClientConfig
{
    public const string DefaultSectionName = "TextExtractor";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string AccessKey { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TimeoutSeconds { get; init; } = 200;
}
