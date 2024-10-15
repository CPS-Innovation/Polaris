using Common.Dto.Response.Document;

namespace coordinator.Validators;

public interface ICmsDocumentsResponseValidator
{
    public bool Validate(CmsDocumentDto[] cmsDocuments);
}