﻿using System;
using Common.Domain.Document;
using Common.Dto.Response.Document;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Durable.Payloads;

public class DocumentPayload : CasePayload
{
    public DocumentPayload()
    {
    }

    public DocumentPayload
    (
        string urn,
        int caseId,
        string documentId,
        long versionId,
        string path,
        DocumentTypeDto documentType,
        DocumentNature.Types documentNatureType,
        DocumentDeltaType documentDeltaType,
        string cmsAuthValues,
        Guid correlationId,
        bool? isOcredProcessedPreference = null
    )
    : base(urn, caseId, cmsAuthValues, correlationId)
    {
        DocumentId = documentId;
        VersionId = versionId;
        DocumentType = documentType;
        Path = path;
        DocumentNatureType = documentNatureType;
        DocumentDeltaType = documentDeltaType;
        IsOcredProcessedPreference = isOcredProcessedPreference;
    }

    public string DocumentId { get; set; }

    public long VersionId { get; set; }

    public string Path { get; set; }

    public DocumentTypeDto DocumentType { get; set; } = new DocumentTypeDto();

    public DocumentNature.Types DocumentNatureType { get; set; }

    public DocumentDeltaType DocumentDeltaType { get; set; }

    public FileType? FileType
    {
        get => DocumentNatureType == DocumentNature.Types.Document
            ? FileTypeHelper.TryGetSupportedFileType(Path, out var fileType) ? fileType : null
            : FileTypeHelper.PseudoDocumentFileType;
    }

    public bool? IsOcredProcessedPreference { get; set; }
}
