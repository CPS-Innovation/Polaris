using System;
using Common.Domain.Document;
using Common.Dto.Response.Document;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Durable.Payloads
{
    public class DocumentPayload : CasePayload
    {
        public DocumentPayload
            (
                string urn,
                int caseId,
                string documentId,
                long versionId,
                string path,
                DocumentTypeDto documentType,
                DocumentNature documentNature,
                DocumentDeltaType documentDeltaType,
                string cmsAuthValues,
                Guid correlationId
            )
            : base(urn, caseId, cmsAuthValues, correlationId)
        {
            DocumentId = documentId;
            VersionId = versionId;
            DocumentType = documentType;
            Path = path;
            DocumentNature = documentNature;
            DocumentDeltaType = documentDeltaType;
        }

        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string Path { get; set; }

        public DocumentTypeDto DocumentType { get; set; } = new DocumentTypeDto();

        public DocumentNature DocumentNature { get; set; }

        public DocumentDeltaType DocumentDeltaType { get; set; }

        public FileType? FileType
        {
            get => DocumentNature == DocumentNature.Document
                ? FileTypeHelper.TryGetSupportedFileType(Path, out var fileType) ? fileType : null
                : FileTypeHelper.PseudoDocumentFileType;
        }
    }
}
