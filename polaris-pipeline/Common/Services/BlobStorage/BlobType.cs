namespace Common.Services.BlobStorage;

public enum BlobType
{
    Pdf,
    Ocr,
    Pii,
    Thumbnail,
    DocumentState,
    CaseState,
    CaseDelta,
    DocumentsList,
    CmsOriginal
}