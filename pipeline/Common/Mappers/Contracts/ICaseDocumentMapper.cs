using Common.Domain.DocumentExtraction;

namespace Common.Mappers.Contracts;

public interface ICaseDocumentMapper<T>
{
    CaseDocument Map(T item);
}
