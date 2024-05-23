namespace coordinator.Domain
{
    public class PiiTextDocumentBatchStatistics
    {
        public int DocumentCount { get; set; }
        public int ValidDocumentCount { get; set; }
        public int InvalidDocumentCount { get; set; }
        public long TransactionCount { get; set; }
    }
}