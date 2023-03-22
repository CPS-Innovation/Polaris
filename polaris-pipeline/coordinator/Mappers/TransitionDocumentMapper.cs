using System;
using Common.Domain.DocumentExtraction;
using coordinator.Domain.Tracker;

namespace coordinator.Mappers
{
    public class TransitionDocumentMapper : ITransitionDocumentMapper
    {
        public TransitionDocument MapToTransitionDocument(CmsCaseDocument document)
        {
            return new TransitionDocument(
                        polarisDocumentId: Guid.NewGuid(),
                      documentId: document.DocumentId,
                      versionId: document.VersionId,
                      originalFileName: document.FileName,
                      mimeType: document.MimeType,
                      fileExtension: document.FileExtension,
                      cmsDocType: document.CmsDocType,
                      createdDate: document.DocumentDate
                      );
        }
    }
}