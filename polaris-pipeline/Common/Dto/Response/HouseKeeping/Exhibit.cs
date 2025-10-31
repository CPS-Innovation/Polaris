// <copyright file="Exhibit.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents an exhibit related to a case, containing details such as the material id,
/// original file name and document type.
/// </summary>
/// <param name="Id">The material Id of the exhibit.</param>
/// <param name="Title">The title of the exhibit.</param>
/// <param name="OriginalFileName">The original file name of the exhibit.</param>
/// <param name="MaterialType">The material type of the statement.</param>
/// <param name="DocumentType">The document type ID of the exhibit, used to identify the document type.</param>
/// <param name="Link">The downloadable link to the exhibit.</param>
/// <param name="Status">The status of an exhibit.</param>
/// <param name="ReceivedDate">The material's received date.</param>
/// <param name="Reference">The reference of the exhibit.</param>
/// <param name="Producer">The producer of the exhibit.</param>
public record Exhibit(
    int Id,
    string Title,
    string OriginalFileName,
    string MaterialType,
    int? DocumentType,
    string Link,
    string Status,
    DateTime? ReceivedDate,
    string Reference,
    string Producer)
    : IMaterial
{
    /// <summary>
    /// Gets or sets the title of the exhibit.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; } = Id;

    /// <summary>
    /// Gets or sets the title of the exhibit.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = Title;

    /// <summary>
    /// Gets or sets the document type of the exhibit.
    /// </summary>
    [JsonPropertyName("documentType")]
    public int? DocumentType { get; set; } = DocumentType;

    /// <summary>
    /// Gets or sets the original file name associated with the exhibit.
    /// </summary>
    [JsonPropertyName("originalFileName")]
    public string OriginalFileName { get; set; } = OriginalFileName;

    /// <summary>
    /// Gets or sets the material type associated with the exhibit.
    /// </summary>
    [JsonPropertyName("materialType")]
    public string MaterialType { get; set; } = MaterialType;

    /// <summary>
    /// Gets or sets the link associated with the exhibit.
    /// </summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = Link;

    /// <summary>
    /// Gets the material ID of the exhibit.
    /// </summary>
    public int MaterialId => Id;

    /// <summary>
    /// Gets or sets the status of the material.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = Status;

    /// <summary>
    /// Gets or sets the exhibit's received date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime? ReceivedDate { get; set; } = ReceivedDate;

    /// <summary>
    /// Gets or sets the exhibit reference.
    /// </summary>
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = Reference;

    /// <summary>
    /// Gets or sets the exhibit producer.
    /// </summary>
    [JsonPropertyName("producer")]
    public string Producer { get; set; } = Producer;
}
