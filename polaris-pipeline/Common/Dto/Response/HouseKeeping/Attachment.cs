// <copyright file="Attachment.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Text.Json.Serialization;

/// <summary>
/// Represents an attachment to a communication, containing details such as the material ID,
/// original file name, document type, and various related metadata.
/// </summary>
/// <param name="MaterialId">The unique material ID associated with the communication.</param>
/// <param name="Name">The name of the attachment.</param>
/// <param name="Description">A textual description of the attachment.</param>
/// <param name="Link">A URL link pointing to the attachment.</param>
/// <param name="Classification">The classification label of the attachment, indicating its category or type.</param>
/// <param name="DocumentTypeId">The document type ID that further identifies the attachment.</param>
/// <param name="NumOfDocVersions">The total number of document versions available for the attachment.</param>
/// <param name="Statement">A statement or content associated with the attachment.</param>
/// <param name="Exhibit">The exhibit information related to the attachment.</param>
/// <param name="Tag">A tag or label applied to the attachment for organization.</param>
/// <param name="DocId">The unique document ID associated with the attachment.</param>
/// <param name="OriginalFileName">The original file name of the attachment when uploaded.</param>
/// <param name="CheckedOutTo">The name or identifier of the user who has checked out the attachment for edits.</param>
/// <param name="DocumentId">The document ID that further identifies the attachment.</param>
/// <param name="OcrProcessed">Indicates whether the attachment has been processed for Optical Character Recognition (OCR).</param>
/// <param name="Direction">The communication direction of the attachment (e.g., inbound or outbound).</param>
public record Attachment(
    [property: JsonPropertyName("materialId")] int MaterialId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("classification")] string Classification,
    [property: JsonPropertyName("documentTypeId")] int DocumentTypeId,
    [property: JsonPropertyName("numOfDocVersions")] int? NumOfDocVersions,
    [property: JsonPropertyName("statement")] StatementAttachmentSubType? Statement,
    [property: JsonPropertyName("exhibit")] ExhibitAttachmentSubType? Exhibit,
    [property: JsonPropertyName("tag")] string Tag,
    [property: JsonPropertyName("docId")] int? DocId,
    [property: JsonPropertyName("originalFileName")] string OriginalFileName,
    [property: JsonPropertyName("checkedOutTo")] string CheckedOutTo,
    [property: JsonPropertyName("documentId")] int? DocumentId,
    [property: JsonPropertyName("ocrProcessed")] string OcrProcessed,
    [property: JsonPropertyName("direction")] string Direction);
