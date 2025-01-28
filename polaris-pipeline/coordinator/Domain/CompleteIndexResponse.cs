namespace coordinator.Domain;

public class CompleteIndexResponse
{
    public CompleteIndexResponse()
    {
    }

    public bool IsCompleted { get; set; }

    public long LineCount { get; set; }
}