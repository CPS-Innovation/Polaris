using System.Collections.Generic;

namespace coordinator.Durable.Payloads.Domain
{
    public class CaseDeltasEntity
    {
        // Cms Document
        public List<(CmsDocumentEntity, DocumentDeltaType)> CreatedCmsDocuments { get; set; }
        public List<(CmsDocumentEntity, DocumentDeltaType)> UpdatedCmsDocuments { get; set; }
        public List<CmsDocumentEntity> DeletedCmsDocuments { get; set; }

        // PCD Requests
        public List<PcdRequestEntity> CreatedPcdRequests { get; set; }
        public List<PcdRequestEntity> UpdatedPcdRequests { get; set; }
        public List<PcdRequestEntity> DeletedPcdRequests { get; set; }

        // Defendants And Charges
        public DefendantsAndChargesEntity CreatedDefendantsAndCharges { get; set; }
        public DefendantsAndChargesEntity UpdatedDefendantsAndCharges { get; set; }
        public bool IsDeletedDefendantsAndCharges { get; set; }

        public int CmsDocsProcessedCount
        {
            get
            {
                return CreatedCmsDocuments?.Count ?? 0
                    + UpdatedCmsDocuments?.Count ?? 0;
            }
        }

        public int PcdRequestsProcessedCount
        {
            get
            {
                return CreatedPcdRequests?.Count ?? 0
                    + UpdatedPcdRequests?.Count ?? 0;
            }
        }
    }
}
