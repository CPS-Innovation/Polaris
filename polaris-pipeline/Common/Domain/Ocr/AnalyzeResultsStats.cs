namespace Common.Domain.Ocr;

public class AnalyzeResultsStats
{
    public AnalyzeResultsStats()
    {

    }

    public long PageCount { get; set; }

    public long LineCount { get; set; }

    public long WordCount { get; set; }


    public static AnalyzeResultsStats FromAnalyzeResults(AnalyzeResults analyzeResults) => new ()
    {
        PageCount = analyzeResults.PageCount,
        LineCount = analyzeResults.LineCount,
        WordCount = analyzeResults.WordCount
    };
}