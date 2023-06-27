using Common.Dto.Document;

namespace Common.Mappers.Contracts;

public interface ICaseDocumentMapper<T>
{
    CmsDocumentDto Map(T item);
}
