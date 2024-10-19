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

        public DefendantsAndChargesEntity(long cmsDocumentId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(cmsDocumentId, 1, defendantsAndCharges.PresentationFlags)
        {
            DefendantsAndCharges = defendantsAndCharges;
        }

        public override string DocumentId
        {
            get => $"{DocumentNature.DefendantsAndChargesPrefix}-{CmsDocumentId}";
        }

        public DefendantsAndChargesListDto DefendantsAndCharges { get; set; }

        public bool HasMultipleDefendants => DefendantsAndCharges != null && DefendantsAndCharges.DefendantsAndCharges.Count() > 1;

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
            get => DateTime.Today.ToString("yyyy-MM-dd");
        }

        public DocumentTypeDto CmsDocType { get; } = new DocumentTypeDto("DAC", null, "Review");
    }
}