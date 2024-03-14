using Common.Dto.Document;

namespace coordinator.Clients.Ddei.Mappers;

public interface ICaseDocumentMapper<T>
{
    CmsDocumentDto Map(T item);
}
