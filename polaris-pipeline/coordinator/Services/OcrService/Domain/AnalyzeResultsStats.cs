namespace coordinator.Services.OcrService.Domain
{
    public class AnalyzeResultsStats
    {

        public long PageCount { get; set; }

        public long LineCount { get; set; }

        public long WordCount { get; set; }


        public static AnalyzeResultsStats FromAnalyzeResults(AnalyzeResults analyzeResults)
        {
            return new AnalyzeResultsStats
            {
                PageCount = analyzeResults.PageCount,
                LineCount = analyzeResults.LineCount,
                WordCount = analyzeResults.WordCount
            };
        }
    }
}