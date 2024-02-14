namespace Common.Dto.Response
{
    public class SearchIndexCountResult
    {
        public SearchIndexCountResult(long lineCount)
        {
            LineCount = lineCount;
        }

        public long LineCount { get; protected set; }
    }
}