using Common.Domain.DocumentExtraction;

namespace Common.Mappers.Contracts;

public interface ICaseDocumentMapper<T>
{
    CmsCaseDocument Map(T item);
}
