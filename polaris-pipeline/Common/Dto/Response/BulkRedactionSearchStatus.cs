namespace Common.Dto.Response;

public enum BulkRedactionSearchStatus
{
    NotStarted,
    GeneratingOcrDocument,
    SearchingDocument,
    Completed, 
    Failed
}