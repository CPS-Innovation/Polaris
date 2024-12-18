
using Common.Domain.Document;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Dto.Response.Documents;
using System;

namespace PolarisGateway.Services.DdeiOrchestration.Mappers;

public class DocumentDtoMapper : IDocumentDtoMapper
{
    public DocumentDto Map(CmsDocumentDto document, PresentationFlagsDto presentationFlagsDto)
    {
        return new DocumentDto
        {
            DocumentId = DocumentNature.ToQualifiedStringDocumentId(document.DocumentId, DocumentNature.Types.Document),
            VersionId = document.VersionId,
            CmsDocType = document.CmsDocType,
            CmsFileCreatedDate = document.DocumentDate,
            CmsOriginalFileName = document.FileName,
            PresentationTitle = document.PresentationTitle,
            IsOcrProcessed = document.IsOcrProcessed,
            CategoryListOrder = document.CategoryListOrder,
            ParentDocumentId = document.ParentDocumentId,
            WitnessId = document.WitnessId,
            PresentationFlags = presentationFlagsDto,
            HasFailedAttachments = document.HasFailedAttachments,
            HasNotes = document.HasNotes,
            IsUnused = document.IsUnused,
            IsInbox = document.IsInbox,
            Classification = document.Classification,
            IsWitnessManagement = document.IsWitnessManagement,
            CanReclassify = document.CanReclassify,
            CanRename = document.CanRename,
            RenameStatus = document.RenameStatus,
            Reference = document.Reference
        };
    }

    public DocumentDto Map(PcdRequestCoreDto pcdRequest, PresentationFlagsDto presentationFlagsDto)
    {
        var documentId = DocumentNature.ToQualifiedStringDocumentId(pcdRequest.Id, DocumentNature.Types.PreChargeDecisionRequest);

        return new DocumentDto
        {
            DocumentId = documentId,
            CmsDocType = new DocumentTypeDto("PCD", null, "Review"),
            CmsFileCreatedDate = pcdRequest.DecisionRequested,
            CmsOriginalFileName = $"{documentId}.pdf",
            PresentationTitle = documentId,
            PresentationFlags = presentationFlagsDto
        };
    }

    public DocumentDto Map(DefendantsAndChargesListDto defendantAndCharges, PresentationFlagsDto presentationFlagsDto)
    {
        var documentId = DocumentNature.ToQualifiedStringDocumentId(defendantAndCharges.CaseId, DocumentNature.Types.DefendantsAndCharges);

        return new DocumentDto
        {
            DocumentId = documentId,
            VersionId = defendantAndCharges.VersionId,
            CmsDocType = new DocumentTypeDto("DC", null, "Review"),
            // this date is never displayed, and is not used for any logic
            CmsFileCreatedDate = new DateTime(1970, 1, 1).ToString("yyyy-MM-dd"),
            CmsOriginalFileName = $"{documentId}.pdf",
            PresentationTitle = documentId,
            PresentationFlags = presentationFlagsDto
        };
    }
}