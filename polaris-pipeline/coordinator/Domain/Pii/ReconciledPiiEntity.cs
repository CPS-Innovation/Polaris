using coordinator.Services.OcrResultsService;

namespace coordinator.Domain
{
    public class ReconciledPiiEntity
    {
        public ReconciledPiiEntity(OcrLineResult ocrLineResult, OcrWord ocrWord, string piiCategory, string polarisDocumentId)
        {
            PageHeight = ocrLineResult.PageHeight;
            PageWidth = ocrLineResult.PageWidth;
            PageIndex = ocrLineResult.PageIndex;
            LineIndex = ocrLineResult.LineIndex;
            AccumulativeLineIndex = ocrLineResult.AccumulativeLineIndex;
            LineText = ocrLineResult.Text;
            Word = ocrWord;
            PiiCategory = piiCategory;
            PolarisDocumentId = polarisDocumentId;
        }

        public OcrWord Word { get; protected set; }
        public string PolarisDocumentId { get; protected set; }
        public double PageHeight { get; protected set; }
        public double PageWidth { get; protected set; }
        public int PageIndex { get; protected set; }
        public int LineIndex { get; protected set; }
        public int AccumulativeLineIndex { get; protected set; }
        public string LineText { get; protected set; }
        public string PiiCategory { get; protected set; }
    }
}