using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Dto.Response.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Mappers
{
    public class CmsDocumentDtoMapper : ICmsDocumentDtoMapper
    {
        public CmsDocumentDto Map(DocumentDto document, PresentationFlagsDto presentationFlagsDto)
        {
            return new CmsDocumentDto
            {
                DocumentId = long.TryParse(document.DocumentId, out var documentId)
                    ? documentId
                    : 0,
                VersionId = document.VersionId,
                FileName = document.CmsOriginalFileName,
                PresentationTitle = document.PresentationTitle,
                CmsDocType = document.CmsDocType,
                DocumentDate = document.CmsFileCreatedDate,
                IsOcrProcessed = document.IsOcrProcessed,
                CategoryListOrder = document.CategoryListOrder,
                PresentationFlags = presentationFlagsDto,
                ParentDocumentId = document.ParentDocumentId,
                WitnessId = document.WitnessId,
                HasFailedAttachments = document.HasFailedAttachments,
                HasNotes = document.HasNotes,
                IsUnused = document.IsUnused,
                IsInbox = document.IsInbox,
                Classification = document.Classification,
                IsWitnessManagement = document.IsWitnessManagement,
                CanReclassify = document.CanReclassify,
                CanRename = document.CanRename,
                RenameStatus = document.RenameStatus,
                Reference = document.Reference,
                Title = document.Title,
                FileExtension = document.FileExtension,
                MimeType = document.MimeType,
                Path = document.Path,
            };
        }
    }

}
