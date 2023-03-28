using Common.Dto.Document;

namespace Common.Mappers.Contracts;

public interface ICaseDocumentMapper<T>
{
    DocumentDto Map(T item);
}
