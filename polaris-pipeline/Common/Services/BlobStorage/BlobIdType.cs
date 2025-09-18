namespace Common.Services.BlobStorage;

public record struct BlobIdType(int CaseId, string DocumentId, long VersionId, BlobType BlobType, string SearchText = null);