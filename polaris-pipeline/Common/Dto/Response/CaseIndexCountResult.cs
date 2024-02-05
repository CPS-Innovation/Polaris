namespace Common.Dto.Response
{
    public class CaseIndexCountResult
    {
        public CaseIndexCountResult(long lineCount)
        {
            LineCount = lineCount;
        }

        public long LineCount { get; protected set; }
    }
}