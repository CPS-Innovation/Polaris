using System;
using System.IO;
using System.Linq;
using Common.Constants;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;

namespace coordinator.Durable.Payloads.Domain
{
    public class DefendantsAndChargesEntity : BaseDocumentEntity
    {
        public DefendantsAndChargesEntity()
        { }

        public DefendantsAndChargesEntity(long cmsDocumentId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(cmsDocumentId, 1, defendantsAndCharges.PresentationFlags)
        {
            DefendantsAndCharges = defendantsAndCharges;
        }

        public override string DocumentId
        {
            get => $"{PolarisDocumentTypePrefixes.DefendantsAndCharges}-{CmsDocumentId}";
        }

        public DefendantsAndChargesListDto DefendantsAndCharges { get; set; }

        public bool HasMultipleDefendants => DefendantsAndCharges != null && DefendantsAndCharges.DefendantsAndCharges.Count() > 1;

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

        public static string CmsFileCreatedDate
        {
            get => DateTime.Today.ToString("yyyy-MM-dd");
        }

        public DocumentTypeDto CmsDocType { get; } = new DocumentTypeDto("DAC", null, "Review");
    }
}