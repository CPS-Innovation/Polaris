using coordinator.Services.OcrResultsService;

namespace coordinator.Domain
{
    public class ReconciledPiiEntity
    {
        public ReconciledPiiEntity(OcrLineResult ocrLineResult, OcrWord ocrWord, string piiCategory)
        {
            PageIndex = ocrLineResult.PageIndex;
            LineIndex = ocrLineResult.LineIndex;
            LineText = ocrLineResult.Text;
            Word = ocrWord;
            PiiCategory = piiCategory;
        }

        public OcrWord Word { get; set; }
        public int PageIndex { get; set; }
        public int LineIndex { get; set; }
        public string LineText { get; set; }
        public string PiiCategory { get; set; }
    }
}