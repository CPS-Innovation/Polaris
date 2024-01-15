namespace Common.Services.CaseSearchService
{
    public class IndexDocumentsDeletedResult
    {
        public long DocumentCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }
    }
}