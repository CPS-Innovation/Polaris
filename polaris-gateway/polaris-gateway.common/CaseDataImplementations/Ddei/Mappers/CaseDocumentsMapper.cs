using PolarisGateway.Domain.StaticData;
using Ddei.Domain;
using Common.Dto.Case;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public class CaseDocumentsMapper : ICaseDocumentsMapper
    {
        public DocumentDetailsDto MapDocumentDetails(DdeiDocumentDetailsDto documentDetails)
        {
            var documentCmsType = DocumentCmsTypes.Lookup(documentDetails.TypeId);

            return new DocumentDetailsDto
            {
                DocumentId = documentDetails.Id,
                VersionId = documentDetails.VersionId,
                FileName = documentDetails.OriginalFileName,
                MimeType = documentDetails.MimeType,
                CreatedDate = documentDetails.Date,
                CmsDocCategory = MapCmsDocCategory(documentDetails.CmsDocCategory),
                CmsDocType = documentCmsType ?? DocumentCmsTypeDto.EmptyDocumentCmsType
            };
        }

        private CmsDocCategory MapCmsDocCategory(DdeiCmsDocCategory cmsDocCategory) =>
             (CmsDocCategory)Enum.Parse(typeof(CmsDocCategory), cmsDocCategory.ToString());
    }
}