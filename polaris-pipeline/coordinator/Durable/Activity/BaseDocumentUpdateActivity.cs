using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Durable.Activity;

public abstract class BaseDocumentUpdateActivity()
{
    protected static BaseDocumentEntity GetDocument(string documentId, CaseDurableEntityDocumentsState documentsState)
    {
        var cmsDocument = documentsState.CmsDocuments.Find(doc => doc.MaterialId == documentId);
        if (cmsDocument != null)
        {
            return cmsDocument;
        }

        var pcdRequest = documentsState.PcdRequests.Find(pcd => pcd.MaterialId == documentId);
        if (pcdRequest != null)
        {
            return pcdRequest;
        }

        if (documentsState.DefendantsAndCharges != null)
        {
            return documentsState.DefendantsAndCharges;
        }

        return null;
    }
}
