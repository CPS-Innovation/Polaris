using BusinessDomain = PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.StaticData;
using Ddei.Domain;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public class CaseDocumentsMapper : ICaseDocumentsMapper
    {
        public BusinessDomain.DocumentDetails MapDocumentDetails(DdeiDocumentDetailsDto documentDetails)
        {
            var documentCmsType = DocumentCmsTypes
                .GetDocumentCmsTypes
                .FirstOrDefault(item => item.Id == documentDetails.TypeId);

            return new BusinessDomain.DocumentDetails
            {
                DocumentId = documentDetails.Id,
                VersionId = documentDetails.VersionId,
                FileName = documentDetails.OriginalFileName,
                MimeType = documentDetails.MimeType,
                CreatedDate = documentDetails.Date,
                CmsDocCategory = MapCmsDocCategory(documentDetails.CmsDocCategory),
                CmsDocType = documentCmsType ?? BusinessDomain.DocumentCmsType.EmptyDocumentCmsType
            };
        }

        private BusinessDomain.CmsDocCategory MapCmsDocCategory(DdeiCmsDocCategory cmsDocCategory) =>
             (BusinessDomain.CmsDocCategory)Enum.Parse(typeof(BusinessDomain.CmsDocCategory), cmsDocCategory.ToString());

    }
}