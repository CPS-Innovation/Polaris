
namespace pdf_thumbnail_generator.Domain
{
  public class ThumbnailResponse
  {
    private readonly string _caseUrn;
    private readonly int _caseId;
    private readonly string _documentId;
    private readonly int _versionId;

    public ThumbnailResponse(string caseUrn, int caseId, string documentId, int versionId)
    {
      _caseUrn = caseUrn;
      _caseId = caseId;
      _documentId = documentId;
      _versionId = versionId;
    }
  }
}

