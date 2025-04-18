﻿using coordinator.Domain;
using System.Collections.Generic;

namespace coordinator.Durable.Payloads.Domain
{
    public class CaseDeltasEntity
    {
        public List<DocumentDelta> CreatedCmsDocuments { get; set; }
        public List<DocumentDelta> UpdatedCmsDocuments { get; set; }
        public List<CmsDocumentEntity> DeletedCmsDocuments { get; set; }

        public List<PcdRequestEntity> CreatedPcdRequests { get; set; }
        public List<PcdRequestEntity> UpdatedPcdRequests { get; set; }
        public List<PcdRequestEntity> DeletedPcdRequests { get; set; }

        public DefendantsAndChargesEntity CreatedDefendantsAndCharges { get; set; }
        public DefendantsAndChargesEntity UpdatedDefendantsAndCharges { get; set; }

        public bool IsDeletedDefendantsAndCharges { get; set; }
    }
}