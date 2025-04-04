﻿using System;
using System.Linq;
using Common.Domain.Document;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;

namespace coordinator.Durable.Payloads.Domain
{
    public class DefendantsAndChargesEntity : BaseDocumentEntity
    {
        public DefendantsAndChargesEntity()
        {
        }

        public DefendantsAndChargesEntity(long cmsDocumentId, long versionId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(cmsDocumentId, versionId, defendantsAndCharges.PresentationFlags)
        {
            HasMultipleDefendants = defendantsAndCharges?.DefendantsAndCharges.Count() > 1;
        }

        public override string DocumentId => DocumentNature.ToQualifiedStringDocumentId(CmsDocumentId, DocumentNature.Types.DefendantsAndCharges);

        public bool HasMultipleDefendants { get; set; }

        public string PresentationTitle => DocumentId;

        public string CmsOriginalFileName => $"{DocumentId}.pdf";

        // this date is never displayed, and is not used for any logic
        public static string CmsFileCreatedDate => new DateTime(1970, 1, 1).ToString("yyyy-MM-dd");

        public DocumentTypeDto CmsDocType { get; } = new DocumentTypeDto("DAC", null, "Review");
    }
}