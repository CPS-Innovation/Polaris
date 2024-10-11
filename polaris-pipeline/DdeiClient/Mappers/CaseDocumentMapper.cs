using Common.Dto.Document;
using Common.Dto.Response;
using DdeiClient.Mappers;

namespace Ddei.Mappers;

public class CaseDocumentMapper : ICaseDocumentMapper<DdeiCaseDocumentResponse>
{
    public CmsDocumentDto Map(DdeiCaseDocumentResponse ddeiResponse)
    {
        return new CmsDocumentDto
        {
            DocumentId = ddeiResponse.Id,
            VersionId = ddeiResponse.VersionId,
            Path = ddeiResponse.Path,
            FileName = ddeiResponse.OriginalFileName,
            PresentationTitle = ddeiResponse.PresentationTitle,
            MimeType = ddeiResponse.MimeType,
            FileExtension = ddeiResponse.FileExtension,
            CmsDocType = new DocumentTypeDto(ddeiResponse.DocumentType, ddeiResponse.DocumentTypeId, ddeiResponse.CmsDocCategory),
            DocumentDate = ddeiResponse.DocumentDate,
            CategoryListOrder = ddeiResponse.CategoryListOrder,
            IsOcrProcessed = ddeiResponse.IsOcrProcessed == true,
            IsDispatched = ddeiResponse.IsDispatched,
            ParentDocumentId = ddeiResponse.ParentId.ToString(),
            WitnessId = ddeiResponse.WitnessId,
            HasFailedAttachments = ddeiResponse.HasFailedAttachments,
            HasNotes = ddeiResponse.HasNotes,
            IsUnused = ddeiResponse.IsUnused,
            IsInbox = ddeiResponse.IsInbox,
            Classification = ddeiResponse.Classification,
            IsWitnessManagement = ddeiResponse.IsWitnessManagement,
            CanReclassify = ddeiResponse.CanReclassify,
            CanRename = ddeiResponse.CanRename,
            RenameStatus = ddeiResponse.RenameStatus,
            Reference = ddeiResponse.Reference
        };
    }
}
