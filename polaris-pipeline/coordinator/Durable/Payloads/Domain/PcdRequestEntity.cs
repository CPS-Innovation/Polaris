using System.IO;
using Common.Constants;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;

namespace coordinator.Durable.Payloads.Domain
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(
            long cmsDocumentId,
            long versionId,
            PresentationFlagsDto presentationFlags)
        : base(cmsDocumentId, versionId, presentationFlags)
        { }

        public PcdRequestEntity(long cmsDocumentId, PcdRequestDto pcdRequest)
            : base(cmsDocumentId, 1, pcdRequest.PresentationFlags)
        {
            PcdRequest = pcdRequest;
        }

        public override string DocumentId
        {
            get => $"{PolarisDocumentTypePrefixes.PcdRequest}-{CmsDocumentId}";
        }

        public PcdRequestDto PcdRequest { get; set; }

        public string PresentationTitle
        {
            get => Path.GetFileNameWithoutExtension(PdfBlobName)
                        // Temporary hack: we need to rationalise the way these are named.  In the meantime, to prevent
                        //  false-positive name update notifications being shown in the UI, we make sure the interim name
                        //  on th PCS request is the same as the eventual name derived from the blob name.
                        ?? DocumentId;
        }

        public string CmsOriginalFileName
        {
            get => Path.GetFileName(PdfBlobName) ?? $"{DocumentId}.pdf";
        }

        public string CmsFileCreatedDate
        {
            get => PcdRequest.DecisionRequested;
        }

        public DocumentTypeDto CmsDocType { get; } = new DocumentTypeDto("PCD", null, "Review");
    }
}