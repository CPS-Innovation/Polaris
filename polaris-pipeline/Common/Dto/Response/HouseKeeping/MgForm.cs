// <copyright file="MgForm.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents an mg form related to a case, containing details such as the material id,
/// original file name and document type.
/// </summary>
/// <param name="Id">The material Id of the mg form.</param>
/// <param name="Title">The title of the mg form.</param>
/// <param name="OriginalFileName">The original file name of the mg form.</param>
/// <param name="MaterialType">The material type of the mg form.</param>
/// <param name="DocumentType">The document type ID of the mg form, used to identify the document type.</param>
/// <param name="Link">The downloadable link to the statement.</param>
/// <param name="Status">The status of the material.</param>
/// <param name="Date">The date that the material was received."></param>
public record MgForm(
    int Id,
    string Title,
    string OriginalFileName,
    string MaterialType,
    int? DocumentType,
    string Link,
    string Status,
    DateTime? Date)
    : IMaterial
{
    /// <summary>
    /// Gets or sets the title of the mg form.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; } = Id;

    /// <summary>
    /// Gets or sets the title of the mg form.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = Title;

    /// <summary>
    /// Gets or sets the document type of the mg form.
    /// </summary>
    [JsonPropertyName("documentType")]
    public int? DocumentType { get; set; } = DocumentType;

    /// <summary>
    /// Gets or sets the original file name associated with the mg form.
    /// </summary>
    [JsonPropertyName("originalFileName")]
    public string OriginalFileName { get; set; } = OriginalFileName;

    /// <summary>
    /// Gets or sets the material type associated with the mg form.
    /// </summary>
    [JsonPropertyName("materialType")]
    public string MaterialType { get; set; } = MaterialType;

    /// <summary>
    /// Gets or sets the link associated with the mg form.
    /// </summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = Link;

    /// <summary>
    /// Gets the material ID of the mg form.
    /// </summary>
    public int MaterialId => Id;

    /// <summary>
    /// Gets or sets the link associated with the mg form.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = Status;

    /// <summary>
    /// Gets or sets the date that the material was created.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime? Date { get; set; } = Date;
}
