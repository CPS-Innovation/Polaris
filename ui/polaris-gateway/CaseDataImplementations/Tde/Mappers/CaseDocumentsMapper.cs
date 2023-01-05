using System.Linq;
using BusinessDomain = RumpoleGateway.Domain.CaseData;
using ApiDomain = RumpoleGateway.CaseDataImplementations.Tde.Domain;
using System;
using RumpoleGateway.Domain.StaticData;

namespace RumpoleGateway.CaseDataImplementations.Tde.Mappers
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
                CmsDocType = documentCmsType
            };
        }

        private BusinessDomain.CmsDocCategory MapCmsDocCategory(ApiDomain.CmsDocCategory cmsDocCategory) =>
             (BusinessDomain.CmsDocCategory)Enum.Parse(typeof(BusinessDomain.CmsDocCategory), cmsDocCategory.ToString());

    }
}