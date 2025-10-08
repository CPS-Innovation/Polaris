// <copyright file="Statement.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a statement related to a case, containing details such as the material id,
/// original file name and document type.
/// </summary>
/// <param name="Id">The material Id of the statement.</param>
/// <param name="WitnessId">The witness Id of the statement.</param>
/// <param name="Title">The title of the statement.</param>
/// <param name="OriginalFileName">The original file name of the statement.</param>
/// <param name="MaterialType">The material type of the statement.</param>
/// <param name="DocumentType">The document type ID of the statement, used to identify the document type.</param>
/// <param name="Link">The downloadable link to the statement.</param>
/// <param name="Status">The status of the material.</param>
/// <param name="ReceivedDate">The date that the material was received.</param>
/// <param name="StatementTakenDate">The date that the statement was taken.</param>
public record Statement(
    int Id,
    int? WitnessId,
    string Title,
    string OriginalFileName,
    string MaterialType,
    int? DocumentType,
    string Link,
    string Status,
    DateTime? ReceivedDate,
    DateTime? StatementTakenDate)
    : IMaterial
{
    /// <summary>
    /// Gets or sets the title of the statement.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; } = Id;

    /// <summary>
    /// Gets or sets the witness id of the statement.
    /// </summary>
    [JsonPropertyName("witnessId")]
    public int? WitnessId { get; set; } = WitnessId;

    /// <summary>
    /// Gets or sets the title of the statement.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = Title;

    /// <summary>
    /// Gets or sets the document type of the statement.
    /// </summary>
    [JsonPropertyName("documentType")]
    public int? DocumentType { get; set; } = DocumentType;

    /// <summary>
    /// Gets or sets the original file name associated with the statement.
    /// </summary>
    [JsonPropertyName("originalFileName")]
    public string OriginalFileName { get; set; } = OriginalFileName;

    /// <summary>
    /// Gets or sets the material type associated with the statement.
    /// </summary>
    [JsonPropertyName("materialType")]
    public string MaterialType { get; set; } = MaterialType;

    /// <summary>
    /// Gets or sets the link associated with the statement.
    /// </summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = Link;

    /// <summary>
    /// Gets the material ID of the statement.
    /// </summary>
    public int MaterialId => Id;

    /// <summary>
    /// Gets or sets the read/unread status of the material.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = Status;

    /// <summary>
    /// Gets or sets the date that the statement was received.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime? ReceivedDate { get; set; } = ReceivedDate;

    /// <summary>
    /// Gets or sets the date that the statement was taken.
    /// </summary>
    [JsonPropertyName("statementTakenDate")]
    public DateTime? StatementTakenDate { get; set; } = StatementTakenDate;

    /// <summary>
    /// Gets the presentation title derived from the original file name, without the file extension.
    /// </summary>
    public string PresentationTitle => Path.GetFileNameWithoutExtension(OriginalFileName);
}
