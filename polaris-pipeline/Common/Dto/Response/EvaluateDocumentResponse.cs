using Common.Constants;

namespace Common.Dto.Response
{
    public class EvaluateDocumentResponse
    {
        public EvaluateDocumentResponse(long caseId, string documentId, long versionId)
        {
            CaseId = caseId;
            DocumentId = documentId;
            VersionId = versionId;
        }

        public DocumentEvaluationResult EvaluationResult { get; set; }

        public long CaseId { get; set; }

        public string DocumentId { get; set; }

        public long VersionId { get; set; }
    }
}
