namespace Common.Domain.DocumentExtraction
{
	public class CaseDocument
	{
		public CaseDocument() { }
		
		public CaseDocument(string documentId, long versionId, string fileName, string documentType, string documentTypeId, string documentCategory)
		{
			DocumentId = documentId;
			VersionId = versionId;
			FileName = fileName;
			CmsDocType = new CmsDocType(documentType, documentTypeId, documentCategory);
		}

		public string DocumentId { get; set; }

		public long VersionId { get; set; }

		public string FileName { get; set; }

		public CmsDocType CmsDocType { get; set; }
	}
}

