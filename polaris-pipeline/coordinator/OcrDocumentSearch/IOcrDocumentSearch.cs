using Common.Domain.Ocr;
using coordinator.Domain;

namespace coordinator.OcrDocumentSearch;

public interface IOcrDocumentSearch
{
    OcrDocumentSearchResponse Search(string searchText, AnalyzeResults results);
}