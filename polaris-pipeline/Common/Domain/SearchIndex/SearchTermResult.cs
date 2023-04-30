namespace Common.Domain.SearchIndex;

public class SearchTermResult
{
    public SearchTermResult(bool termFound, StreamlinedMatchType matchType)
    {
        TermFound = termFound;
        SearchMatchType = matchType;
    }
    
    public bool TermFound { get; }

    public StreamlinedMatchType SearchMatchType { get; }
}