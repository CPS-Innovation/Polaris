using System;
using Common.Dto.Document;
using Common.Mappers.Contracts;

namespace Common.Mappers
{
    public class TransitionDocumentMapper : ITransitionDocumentMapper
    {
        public TransitionDocumentDto MapToTransitionDocument(DocumentDto document)
        {
            return new TransitionDocumentDto
                (
                    polarisDocumentId: Guid.NewGuid(),
                    cmsDocumentId: document.DocumentId,
                    cmsVersionId: document.VersionId,
                    originalFileName: document.FileName,
                    mimeType: document.MimeType,
                    fileExtension: document.FileExtension,
                    cmsDocType: document.CmsDocType,
                    createdDate: document.DocumentDate
                );
        }
    }
}