namespace Common.Dto.Response
{
    public class ExtractTextResponse
    {
        public ExtractTextResponse(long lineCount)
        {
            LineCount = lineCount;
        }

        public long LineCount { get; protected set; }
    }
}