using Common.Dto.Document;

namespace DdeiClient.Mappers;

public interface ICaseDocumentMapper<T>
{
    CmsDocumentDto Map(T item);
}
