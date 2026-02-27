using Common.Dto.Response.Document;
using Ddei.Domain.Response.Document;


namespace Ddei.Mappers;

public class CaseDocumentMapper : ICaseDocumentMapper<MdsDocumentResponse>
{
    public CmsDocumentDto Map(MdsDocumentResponse response)
    {
        return new CmsDocumentDto
        {
            DocumentId = response.Id,
            VersionId = response.VersionId,
            Path = response.Path,
            FileName = response.OriginalFileName,
            PresentationTitle = response.PresentationTitle,
            MimeType = response.MimeType,
            FileExtension = response.FileExtension,
            CmsDocType = new DocumentTypeDto(response.DocumentType, response.DocumentTypeId, response.CmsDocCategory),
            DocumentDate = response.DocumentDate,
            CategoryListOrder = response.CategoryListOrder,
            IsOcrProcessed = response.IsOcrProcessed == true,
            IsDispatched = response.IsDispatched,
            ParentDocumentId = response.ParentId.ToString(),
            WitnessId = response.WitnessId,
            HasFailedAttachments = response.HasFailedAttachments,
            HasNotes = response.HasNotes,
            IsUnused = response.IsUnused,
            IsInbox = response.IsInbox,
            Classification = response.Classification,
            IsWitnessManagement = response.IsWitnessManagement,
            CanReclassify = response.CanReclassify,
            CanRename = response.CanRename,
            RenameStatus = response.RenameStatus,
            Reference = response.Reference,
            Title = response.Title,
        };
    }
}
