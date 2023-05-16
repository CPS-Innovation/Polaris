using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Mapster;
using System.ComponentModel.DataAnnotations;

public class CaseIdentifiers
{
    public int Id { get; set; }

    public string UrnPlusSuffix { get; set; }
}

public class TrackerDto
{
    [JsonProperty("transactionId")]
    public string TransactionId { get; set; }

    [JsonProperty("versionId")]
    public string VersionId { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("status")]
    public TrackerStatus Status { get; set; }

    [JsonProperty("documentsRetrieved")]
    public DateTime? DocumentsRetrieved { get; set; }

    [JsonProperty("processingCompleted")]
    public DateTime? ProcessingCompleted { get; set; }

    [JsonProperty("documents")]
    public List<TrackerCmsDocumentDto> Documents { get; set; }

    [JsonProperty("logs")]
    public List<TrackerLogDto> Logs { get; set; }
}

public class TrackerCmsDocumentDto : BaseTrackerDocumentDto
{
    public TrackerCmsDocumentDto()
        : base()
    { }

    public TrackerCmsDocumentDto(
        Guid polarisDocumentId,
        int polarisDocumentVersionId,
        string cmsDocumentId,
        long cmsVersionId,
        DocumentTypeDto cmsDocType,
        string cmsFileCreatedDate,
        string cmsOriginalFileName,
        PresentationFlagsDto presentationFlags)
        : base(polarisDocumentId, polarisDocumentVersionId, cmsDocumentId, cmsVersionId, presentationFlags)
    {
        CmsDocType = cmsDocType;
        CmsFileCreatedDate = cmsFileCreatedDate;
        CmsOriginalFileName = cmsOriginalFileName;
        Status = TrackerDocumentStatus.New;
    }

    [JsonProperty("cmsDocType")]
    public DocumentTypeDto CmsDocType { get; set; }

    [JsonProperty("cmsOriginalFileName")]
    [Required]
    [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
    public string CmsOriginalFileName { get; set; }

    [JsonProperty("cmsFileCreatedDate")]
    public string CmsFileCreatedDate { get; set; }
}

public class BaseTrackerDocumentDto
{
    public BaseTrackerDocumentDto()
    { }

    public BaseTrackerDocumentDto(
        Guid polarisDocumentId,
        int polarisDocumentVersionId,
        string cmsDocumentId,
        long cmsVersionId,
        PresentationFlagsDto presentationFlags)
    {
        PolarisDocumentId = polarisDocumentId;
        PolarisDocumentVersionId = polarisDocumentVersionId;
        CmsDocumentId = cmsDocumentId;
        CmsVersionId = cmsVersionId;
        PresentationFlags = presentationFlags;
        Status = TrackerDocumentStatus.New;
    }

    [JsonProperty("polarisDocumentId")]
    public Guid PolarisDocumentId { get; set; }

    [JsonProperty("polarisDocumentVersionId")]
    public int PolarisDocumentVersionId { get; set; }

    [JsonProperty("cmsDocumentId")]
    [AdaptIgnore]
    public string CmsDocumentId { get; set; }

    // Todo - don't send to client
    [JsonProperty("cmsVersionId")]
    [AdaptIgnore]
    public long CmsVersionId { get; set; }

    [JsonProperty("pdfBlobName")]
    public string PdfBlobName { get; set; }

    [JsonProperty("isPdfAvailable")]
    public bool IsPdfAvailable { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("status")]
    public TrackerDocumentStatus Status { get; set; }

    [JsonProperty("presentationFlags")]
    public PresentationFlagsDto PresentationFlags { get; set; }
}

public class DocumentTypeDto
{
    public DocumentTypeDto() { }

    public DocumentTypeDto(string documentType, string documentTypeId, string documentCategory)
    {
        DocumentTypeId = documentTypeId ?? MiscCategories.UnknownDocumentType;
        DocumentType = documentType;
        DocumentCategory = documentCategory;
    }

    public string DocumentTypeId { get; set; }

    public string DocumentType { get; set; }

    public string DocumentCategory { get; set; }
}

public enum TrackerStatus
{
    Running,
    DocumentsRetrieved,
    Completed,
    Failed,
    Deleted
}

public static class MiscCategories
{
    public const string UnknownDocumentType = "1029";
}

public class PresentationFlagsDto
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ReadFlag Read { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public WriteFlag Write { get; set; }
}

public enum TrackerDocumentStatus
{
    New,
    Indexed,
    PdfUploadedToBlob,
    UnableToConvertToPdf,
    UnexpectedFailure,
    OcrAndIndexFailure,
    DocumentAlreadyProcessed,
}

public enum ReadFlag
{
    Ok,
    OnlyAvailableInCms
}

public enum WriteFlag
{
    Ok,
    OnlyAvailableInCms,
    DocTypeNotAllowed,
    OriginalFileTypeNotAllowed
}

public class TrackerLogDto
{
    [JsonProperty("logType")]
    public string LogType { get; set; }

    [JsonProperty("timestamp")]
    public string TimeStamp { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("cmsDocumentId", NullValueHandling = NullValueHandling.Ignore)]
    public string CmsDocumentId { get; set; }
}