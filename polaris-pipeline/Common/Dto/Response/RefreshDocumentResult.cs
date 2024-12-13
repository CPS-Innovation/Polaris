namespace Common.Dto.Response
{
    public class RefreshDocumentResult
    {
        public long OcrLineCount { get; set; }

        public static RefreshDocumentResult Empty() => new ()
        {
            OcrLineCount = 0
        };
    }
}