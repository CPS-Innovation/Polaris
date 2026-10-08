// <copyright file="CaseMaterial.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using System;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;

namespace Common.Dto.Response.HouseKeeping;

/// <summary>
/// Represents a case material with various attributes like id, file name, status, etc.
/// </summary>
/// <summary>
/// Represents a case material with various properties related to its metadata.
/// </summary>
public record CaseMaterial
{
    /// <summary>
    /// Gets the unique identifier for the case material.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the material type identifier for the material. Maps to the UI's parentId.
    /// </summary>
    public int MaterialId { get; init; }

    /// <summary>
    /// Gets the document/version identifier for the material. Maps to the UI's childId/versionId.
    /// </summary>
    public int? DocumentId { get; init; }

    /// <summary>
    /// Gets the original file name associated with the case material. Maps to CmsOriginalName.
    /// </summary>
    public string OriginalFileName { get; init; }

    /// <summary>
    /// Gets the subject of the case material. Maps to PresentationTitle.
    /// </summary>
    public string Subject { get; init; }

    /// <summary>
    /// Gets the document type identifier for the case material.
    /// </summary>
    public int DocumentTypeId { get; init; }

    /// <summary>
    /// Gets the category of the case material (e.g., "MG Form").
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// Gets the type of the case material (e.g., "MG3").
    /// </summary>
    public string Type { get; init; }

    /// <summary>
    /// Gets a value indicating whether the case material has attachments.
    /// </summary>
    public bool HasAttachments { get; init; }

    /// <summary>
    /// Gets the current status of the case material (e.g., "Used", "None", "Unused").
    /// </summary>
    public string Status { get; init; }

    /// <summary>
    /// Gets or sets the current read state of the case material (e.g., "Read", "Unread").
    /// </summary>
    public string ReadStatus { get; set; }

    /// <summary>
    /// Gets the type of communication (e.g. email, phone, Bundle).
    /// </summary>
    public string Method { get; init; }

    /// <summary>
    /// Gets the direction of communication (e.g. incoming, outgoing).
    /// </summary>
    public string Direction { get; init; }

    /// <summary>
    /// Gets which party the communication is sitting with (e.g. Counsel, police, DCS).
    /// </summary>
    public string Party { get; init; }

    /// <summary>
    /// Gets the received or sent date of the communication.
    /// </summary>
    public DateTime? Date { get; init; }

    /// <summary>
    /// Gets the date that the statement was taken (only applies to Statements).
    /// </summary>
    public DateTime? RecordedDate { get; init; }

    /// <summary>
    /// Gets the witness associated with the case material.
    /// </summary>
    public int? WitnessId { get; init; }

    /// <summary>
    /// Gets the title/statement number associated with the case material.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the exhibit producer associated with the case material.
    /// </summary>
    public string Producer { get; init; }

    /// <summary>
    /// Gets the exhibit item for the case material.
    /// </summary>
    public string Item { get; init; }

    /// <summary>
    /// Gets existing exhibit producer or witness Id associated with the case material.
    /// </summary>
    public int? ExistingProducerOrWitnessId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the case material can be reclassified or not.
    /// </summary>
    public bool IsReclassifiable { get; init; }

    /// <summary>
    /// Gets the CMS document type details (id, type, category) for the case material.
    /// </summary>
    public DocumentTypeDto CmsDocType { get; init; }

    /// <summary>
    /// Gets the presentation flags (read/write) controlling how the case material can be interacted with.
    /// </summary>
    public PresentationFlagsDto PresentationFlags { get; init; }

    /// <summary>
    /// Gets a value indicating whether the case material has notes associated with it.
    /// </summary>
    public bool HasNotes { get; init; }

    /// <summary>
    /// Gets a value indicating whether the case material is marked as unused.
    /// </summary>
    public bool IsUnused { get; init; }

    /// <summary>
    /// Gets the link to the case material.
    /// </summary>
    public string Link { get; init; }

    /// <summary>
    /// Gets the exhibit reference producer associated with the case material.
    /// </summary>
    public string Reference { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseMaterial"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the case material.</param>
    /// <param name="originalFileName">The original file name associated with the case material.</param>
    /// <param name="subject">The subject of the case material.</param>
    /// <param name="documentTypeId">The document type identifier for the case material.</param>
    /// <param name="materialId">The material type identifier for the material.</param>
    /// <param name="link">The link to the case material.</param>
    /// <param name="category">The category of the case material (e.g., "Exhibit", "Statement").</param>
    /// <param name="type">The type of the case material (e.g., "Used", "Unused").</param>
    /// <param name="hasAttachments">Indicates whether the case material has attachments.</param>
    /// <param name="status">The status of the case material (e.g., "Used", "None", "Unused").</param>
    /// <param name="readStatus">The read state of the material.</param>
    /// <param name="isReclassifiable">a flag indicating whether the case material can be reclassified or not.</param>
    /// <param name="method">The type of communication (e.g. email, phone, Bundle). </param>
    /// <param name="direction">The direction of communication (e.g. incoming, outgoing).</param>
    /// <param name="party">Which party the communication is sitting with (e.g. Counsel, police, DCS).</param>
    /// <param name="date">The received or sent date of the communication.</param>
    /// <param name="recordedDate">The statement taken/recorded date. (only applies to  Statements).</param>
    /// <param name="witnessId">The witness associated with the case material.</param>
    /// <param name="title">The statement number/title associated with the case material.</param>
    /// <param name="reference">The exhibit reference associated with the case material.</param>
    /// <param name="item">The exhibit item associated with the case material.</param>
    /// <param name="producer">The exhibit producer associated with the case material.</param>
    /// <param name="existingProducerOrWitnessId">The exhibit existingProducerOrWitnessId.</param>
    /// <param name="documentId">The document identifier for the case material, as held by the CMS document store.</param>
    /// <param name="cmsDocType">The CMS document type details (id, type, category) for the case material.</param>
    /// <param name="presentationFlags">The presentation flags (read/write) controlling how the case material can be interacted with.</param>
    /// <param name="hasNotes">A value indicating whether the case material has notes associated with it.</param>
    /// <param name="isUnused">A value indicating whether the case material is marked as unused.</param>
    public CaseMaterial(
        int id,
        string originalFileName,
        string subject,
        int documentTypeId,
        int materialId,
        string link,
        string category,
        string type,
        bool hasAttachments,
        string readStatus,
        string status = "None",
        string method = null,
        string direction = null,
        string party = null,
        DateTime? date = null,  
        DateTime? recordedDate = null,
        int? witnessId = null,
        string title = null,
        string reference = null,
        string item = null,
        string producer = null,
        int? existingProducerOrWitnessId = null,
        bool isReclassifiable = false,
        int? documentId = null,
        DocumentTypeDto cmsDocType = null,
        PresentationFlagsDto presentationFlags = null,
        bool hasNotes = false,
        bool isUnused = false)
    {
        Id = id;
        DocumentId = documentId;
        OriginalFileName = originalFileName;
        Subject = subject;
        DocumentTypeId = documentTypeId;
        MaterialId = materialId;
        Link = link;
        Category = category;
        Type = type;
        HasAttachments = hasAttachments;
        ReadStatus = readStatus;
        Method = method;
        Direction = direction;
        Party = party;
        Date = date;
        Status = status;
        RecordedDate = recordedDate;
        WitnessId = witnessId;
        Title = title;
        Reference = reference;
        Item = item;
        Producer = producer;
        ExistingProducerOrWitnessId = existingProducerOrWitnessId;
        IsReclassifiable = isReclassifiable;
        CmsDocType = cmsDocType;
        PresentationFlags = presentationFlags;
        HasNotes = hasNotes;
        IsUnused = isUnused;
    }
}
