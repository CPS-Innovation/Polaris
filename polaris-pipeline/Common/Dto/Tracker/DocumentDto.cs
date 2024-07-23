﻿using System.ComponentModel.DataAnnotations;
using Common.Constants;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Common.ValueObjects;
using Mapster;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Dto.Tracker
{
    public class DocumentDto
    {
        public DocumentDto()
        { }

        [JsonIgnore]
        public PolarisDocumentId PolarisDocumentId { get; set; }

        [JsonProperty("polarisDocumentId")]
        public string PolarisDocumentIdValue
        {
            get
            {
                return PolarisDocumentId.ToString();
            }
            set
            {
                PolarisDocumentId = new PolarisDocumentId(value);
            }
        }

        [JsonProperty("polarisDocumentVersionId")]
        public int PolarisDocumentVersionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("cmsDocumentId")]
        [AdaptIgnore]
        public string CmsDocumentId { get; set; }

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

        [JsonProperty("isPdfAvailable")]
        public bool IsPdfAvailable { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("cmsOriginalFileExtension")]
        public string CmsOriginalFileExtension { get; set; }

        [JsonProperty("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonProperty("categoryListOrder")]
        public int? CategoryListOrder { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }

        [JsonIgnore]
        public PolarisDocumentId PolarisParentDocumentId { get; set; }

        [JsonProperty("polarisParentDocumentId")]
        public string PolarisParentDocumentIdValue
        {
            get
            {
                return PolarisParentDocumentId?.ToString();
            }
            set
            {
                PolarisParentDocumentId = new PolarisDocumentId(value);
            }
        }

        [JsonProperty("cmsParentDocumentId")]
        [JsonIgnore]
        public string CmsParentDocumentId { get; set; }

        [JsonProperty("witnessId")]
        public int? WitnessId { get; set; }

        [JsonProperty("hasFailedAttachments")]
        public bool HasFailedAttachments { get; set; }

        [JsonProperty("hasNotes")]
        public bool HasNotes { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }

        [JsonProperty("piiCmsVersionId")]
        public int? PiiCmsVersionId { get; set; }

        [JsonProperty("isUnused")]
        public bool IsUnused { get; set; }
    }
}