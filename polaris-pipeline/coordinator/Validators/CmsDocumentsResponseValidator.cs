using System.Linq;
using Common.Dto.Document;

namespace coordinator.Validators;

public class CmsDocumentsResponseValidator : ICmsDocumentsResponseValidator
{
    public bool IsValid(CmsDocumentDto[] cmsDocuments)
    {
        // #24476 if we have duplicate document ids then the case orchestrator never completes
        return cmsDocuments.Select(doc => doc.DocumentId)
            .Distinct()
            .Count() == cmsDocuments.Length;
    }
}