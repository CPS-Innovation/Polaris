using Common.Configuration;
using Common.Dto.Response.Documents;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Common.Dto.Response;

namespace coordinator.Services;

public class StateStorageService : IStateStorageService
{
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;

    public StateStorageService(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IConfiguration configuration)
    {
        _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty);
    }

    public async Task<CaseDurableEntityState> GetStateAsync(int caseId)
    {
        var blobId = new BlobIdType(caseId, default, default, BlobType.CaseState);

        return await _polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityState>(blobId) ??
            new CaseDurableEntityState
            {
                Status = CaseRefreshStatus.NotStarted,
                Running = null,
                Retrieved = null,
                Completed = null,
                Failed = null,
                FailedReason = null,
                CaseId = caseId,
            };
    }

    public async Task<bool> UpdateStateAsync(CaseDurableEntityState caseState)
    {
        var blobId = new BlobIdType(caseState.CaseId, default, default, BlobType.CaseState);
        await _polarisBlobStorageService.UploadObjectAsync(caseState, blobId);

        return true;
    }

    public async Task<BulkRedactionSearchEntityState> GetBulkRedactionSearchStateAsync(int caseId, string documentId, long versionId, string searchText)
    {
        var blobId = new BlobIdType(caseId, documentId, versionId, BlobType.BulkRedactionSearchState, searchText);

        return await _polarisBlobStorageService.TryGetObjectAsync<BulkRedactionSearchEntityState>(blobId) ??
               new BulkRedactionSearchEntityState
               {
                   Status = BulkRedactionSearchStatus.NotStarted,
                   CaseId = caseId,
                   DocumentId = documentId,
                   VersionId = versionId,
                   SearchTerm = searchText
               };
    }

    public async Task<bool> UpdateBulkRedactionSearchStateAsync(BulkRedactionSearchEntityState bulkRedactionSearchEntityState)
    {
        var blobId = new BlobIdType(bulkRedactionSearchEntityState.CaseId, bulkRedactionSearchEntityState.DocumentId, bulkRedactionSearchEntityState.VersionId, BlobType.BulkRedactionSearchState, bulkRedactionSearchEntityState.SearchTerm);
        await _polarisBlobStorageService.UploadObjectAsync(bulkRedactionSearchEntityState, blobId);

        return true;
    }

    public async Task<CaseDurableEntityDocumentsState> GetDurableEntityDocumentsStateAsync(int caseId)
    {
        var blobId = new BlobIdType(caseId, default, default, BlobType.DocumentState);

        return (await _polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityDocumentsState>(blobId)) ?? new CaseDurableEntityDocumentsState();
    }

    public async Task<bool> UpdateDurableEntityDocumentsStateAsync(int caseId, CaseDurableEntityDocumentsState caseDurableEntityDocumentsState)
    {
        var blobId = new BlobIdType(caseId, default, default, BlobType.DocumentState);
        await _polarisBlobStorageService.UploadObjectAsync(caseDurableEntityDocumentsState, blobId);

        return true;
    }

    public async Task<CaseDeltasEntity> GetCaseDeltasEntityAsync(int caseId)
    {
        var blobId = new BlobIdType(caseId, default, default, BlobType.CaseDelta);

        return (await _polarisBlobStorageService.TryGetObjectAsync<CaseDeltasEntity>(blobId)) ?? new CaseDeltasEntity();
    }

    public async Task<bool> UpdateCaseDeltasEntityAsync(int caseId, CaseDeltasEntity caseDeltasEntity)
    {
        var blobId = new BlobIdType(caseId, default, default, BlobType.CaseDelta);
        await _polarisBlobStorageService.UploadObjectAsync(caseDeltasEntity, blobId);

        return true;
    }

    public async Task<GetCaseDocumentsResponse> GetCaseDocumentsAsync(int caseId)
    {
        var blobId = new BlobIdType(caseId, default, default, BlobType.DocumentsList);

        return (await _polarisBlobStorageService.TryGetObjectAsync<GetCaseDocumentsResponse>(blobId)) ?? new GetCaseDocumentsResponse();
    }

    public async Task<bool> UpdateCaseDocumentsAsync(int caseId, GetCaseDocumentsResponse caseDocumentsResponse)
    {
        var blobId = new BlobIdType(caseId, default, default, BlobType.DocumentsList);
        await _polarisBlobStorageService.UploadObjectAsync(caseDocumentsResponse, blobId);

        return true;
    }
}
