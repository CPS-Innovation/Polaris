using System;
using System.Linq;
using Common.Domain.Document;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;

namespace coordinator.Durable.Payloads.Domain
{
    public class DefendantsAndChargesEntity : BaseDocumentEntity
    {
        public DefendantsAndChargesEntity()
        { }

        public DefendantsAndChargesEntity(long cmsDocumentId, long versionId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(cmsDocumentId, versionId, defendantsAndCharges.PresentationFlags)
        {
            HasMultipleDefendants = defendantsAndCharges?.DefendantsAndCharges.Count() > 1;
        }

        public override string DocumentId
        {
            get => $"{DocumentNature.DefendantsAndChargesPrefix}-{CmsDocumentId}";
        }

        public bool HasMultipleDefendants { get; set; }

        public string PresentationTitle
        {
            get => DocumentId;
        }

        public string CmsOriginalFileName
        {
            get => $"{DocumentId}.pdf";
        }

        public static string CmsFileCreatedDate
        {
            // this date is never displayed, and is not used for any logic
            get => new DateTime(1970, 1, 1).ToString("yyyy-MM-dd");
        }

        public DocumentTypeDto CmsDocType { get; } = new DocumentTypeDto("DAC", null, "Review");
    }
}