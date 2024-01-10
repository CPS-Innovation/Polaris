using polaris_common.Dto.Document;

namespace polaris_common.Mappers.Contracts;

public interface ICaseDocumentMapper<T>
{
    CmsDocumentDto Map(T item);
}
