using Common.Dto.Response.Document;

namespace Ddei.Mappers;

public interface ICaseDocumentMapper<T>
{
    CmsDocumentDto Map(T item);
}
