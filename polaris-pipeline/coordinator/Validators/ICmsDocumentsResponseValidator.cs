using Common.Dto.Document;

namespace coordinator.Validators;

public interface ICmsDocumentsResponseValidator
{
    public bool IsValid(CmsDocumentDto[] cmsDocuments);
}