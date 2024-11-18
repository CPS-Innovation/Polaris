namespace pdf_thumbnail_generator.Domain
{
    public class ThumbnailResponse 
    {
        public string CaseUrn { get; }
        public int CaseId { get; }
        public string DocumentId { get; }
        public int VersionId { get; }

        public ThumbnailResponse(string caseUrn, int caseId, string documentId, int versionId)
        { 
            CaseUrn = caseUrn;
            CaseId = caseId;
            DocumentId = documentId;
            VersionId = versionId;
        }
    }
}
