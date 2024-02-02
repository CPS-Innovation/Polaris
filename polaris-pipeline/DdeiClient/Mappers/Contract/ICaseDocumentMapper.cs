using Common.Dto.Document;

namespace DdeiClient.Mappers.Contract;

public interface ICaseDocumentMapper<T>
{
    CmsDocumentDto Map(T item);
}
