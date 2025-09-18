using Common.Configuration;
using System.Text.Json.Serialization;

namespace coordinator.Domain;

public class BulkRedactionSearchResponse(string caseUrn, int caseId, string documentId, long versionId, string searchText)
{
    private string CaseUrn { get; } = caseUrn;
    private int CaseId { get; } = caseId;
    private string DocumentId { get; } = documentId;
    private long VersionId { get; } = versionId;
    private string SearchText { get; } = searchText;

    [JsonPropertyName("trackerUrl")]
    public string TrackerUrl => "/api/" + RestApi.GetBulkRedactionSearchTrackerPath(CaseUrn, CaseId, DocumentId, VersionId, SearchText);
}