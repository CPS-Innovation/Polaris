using System.Linq;
using Common.Dto.Response.Document;

namespace coordinator.Validators;

public class CmsDocumentsResponseValidator : ICmsDocumentsResponseValidator
{
    public bool Validate(CmsDocumentDto[] cmsDocuments)
    {
        // #24476 if we have duplicate document ids then the case orchestrator never completes
        return cmsDocuments.Select(doc => doc.DocumentId)
            .Distinct()
            .Count() == cmsDocuments.Length;
    }
}