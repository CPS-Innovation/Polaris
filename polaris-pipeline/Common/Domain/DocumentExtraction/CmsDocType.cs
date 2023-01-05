﻿using Common.Constants;

namespace Common.Domain.DocumentExtraction
{
	public class CmsDocType
	{
		public CmsDocType() { }
		
		public CmsDocType(string documentType, string documentTypeId, string documentCategory)
		{
			DocumentTypeId = documentTypeId ?? MiscCategories.UnknownDocumentType;
			DocumentType = documentType;
			DocumentCategory = documentCategory;
		}
		
		public string DocumentTypeId { get; set; }

		public string DocumentType { get; set; }

		public string DocumentCategory { get; set; }
	}
}

