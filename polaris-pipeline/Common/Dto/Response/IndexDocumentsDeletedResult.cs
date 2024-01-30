namespace Common.Dto.Response
{
    public class IndexDocumentsDeletedResult
    {
        public long DocumentCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }

        public static IndexDocumentsDeletedResult Empty() => new IndexDocumentsDeletedResult
        {
            DocumentCount = 0,
            SuccessCount = 0,
            FailureCount = 0
        };
    }
}