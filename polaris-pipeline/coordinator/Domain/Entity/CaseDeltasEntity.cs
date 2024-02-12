using System.Collections.Generic;
using System.Linq;

namespace coordinator.Domain.Entity
{
    public class CaseDeltasEntity
    {
        // Cms Document
        public List<CmsDocumentEntity> CreatedCmsDocuments { get; set; }
        public List<CmsDocumentEntity> UpdatedCmsDocuments { get; set; }
        public List<CmsDocumentEntity> DeletedCmsDocuments { get; set; }

        // PCD Requests
        public List<PcdRequestEntity> CreatedPcdRequests { get; set; }
        public List<PcdRequestEntity> UpdatedPcdRequests { get; set; }
        public List<PcdRequestEntity> DeletedPcdRequests { get; set; }

        // Defendants And Charges
        public DefendantsAndChargesEntity CreatedDefendantsAndCharges { get; set; }
        public DefendantsAndChargesEntity UpdatedDefendantsAndCharges { get; set; }
        public bool IsDeletedDefendantsAndCharges { get; set; }

        public int CreatedCount { get { return CreatedCmsDocuments.Count + CreatedPcdRequests.Count + (CreatedDefendantsAndCharges != null ? 1 : 0); } }
        public int UpdatedCount { get { return UpdatedCmsDocuments.Count + UpdatedPcdRequests.Count + (UpdatedDefendantsAndCharges != null ? 1 : 0); } }
        public int DeletedCount { get { return DeletedCmsDocuments.Count + DeletedPcdRequests.Count + (IsDeletedDefendantsAndCharges ? 1 : 0); } }

        public bool Any()
        {
            return CreatedCmsDocuments.Any() || UpdatedCmsDocuments.Any() || DeletedCmsDocuments.Any() ||
                   CreatedPcdRequests.Any() || UpdatedPcdRequests.Any() || DeletedPcdRequests.Any() ||
                   CreatedDefendantsAndCharges != null || UpdatedDefendantsAndCharges != null || IsDeletedDefendantsAndCharges;
        }

        public string GetLogMessage()
        {
            return $"Refresh Documents, " +
                                $"CMS:({CreatedCmsDocuments.Count} created, {UpdatedCmsDocuments.Count} updated, {DeletedCmsDocuments.Count} deleted), " +
                                $"PCD :({CreatedPcdRequests.Count} created, {DeletedPcdRequests.Count} deleted), " +
                                $"DAC :({(CreatedDefendantsAndCharges != null ? 1 : 0)} created, {(UpdatedDefendantsAndCharges != null ? 1 : 0)} deleted).";
        }
    }
}
