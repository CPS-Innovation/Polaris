using Common.Domain.Ocr;
using coordinator.Domain;

namespace coordinator.Search;

public interface IOcrDocumentSearch
{
    OcrDocumentSearchResponse Search(string searchText, AnalyzeResults results);
}