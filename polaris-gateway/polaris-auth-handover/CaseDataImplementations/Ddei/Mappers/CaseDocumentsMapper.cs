using System;
using System.Linq;
using PolarisAuthHandover.Domain.StaticData;
using BusinessDomain = PolarisAuthHandover.Domain.CaseData;
using ApiDomain = PolarisAuthHandover.CaseDataImplementations.Ddei.Domain;

namespace PolarisAuthHandover.CaseDataImplementations.Ddei.Mappers
{
    public class CaseDocumentsMapper : ICaseDocumentsMapper
    {
        public BusinessDomain.DocumentDetails MapDocumentDetails(ApiDomain.DocumentDetails documentDetails)
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

        private BusinessDomain.CmsDocCategory MapCmsDocCategory(ApiDomain.CmsDocCategory cmsDocCategory) =>
             (BusinessDomain.CmsDocCategory)Enum.Parse(typeof(BusinessDomain.CmsDocCategory), cmsDocCategory.ToString());

    }
}