using Newtonsoft.Json;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using System.ComponentModel.DataAnnotations;
using Common.ValueObjects;
using Common.Dto.Tracker;

namespace Common.Domain.Entity
{
    public class CmsDocumentEntity : BaseDocumentEntity
    {
        public CmsDocumentEntity()
            : base()
        { }

        public CmsDocumentEntity(
            PolarisDocumentId polarisDocumentId,
            int polarisDocumentVersionId,
            string cmsDocumentId,
            long cmsVersionId,
            DocumentTypeDto cmsDocType,
            string filePath,
            string fileExtension,
            string cmsFileCreatedDate,
            string cmsOriginalFileName,
            string presentationTitle,
            bool isOcrProcessed,
            int? categoryListOrder,
            PresentationFlagsDto presentationFlags)
            : base(polarisDocumentId, polarisDocumentVersionId, cmsDocumentId, cmsVersionId, presentationFlags)
        {
            CmsDocType = cmsDocType;
            Path = filePath;
            FileExtension = fileExtension;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            PresentationTitle = presentationTitle;
            IsOcrProcessed = isOcrProcessed;
            CategoryListOrder = categoryListOrder;
            Status = DocumentStatus.New;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonProperty("categoryListOrder")]
        public int? CategoryListOrder { get; set; }
    }
}