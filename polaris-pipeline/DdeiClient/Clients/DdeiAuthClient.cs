using Common.Dto.Response.Document;
using Common.Extensions;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Domain.Response.Document;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Factories;

namespace DdeiClient.Clients;

public class DdeiAuthClient : BaseCmsClient, IDdeiAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly IDdeiAuthClientRequestFactory _ddeiAuthClientRequestFactory;
    public DdeiAuthClient(
        HttpClient httpClient,
        IDdeiAuthClientRequestFactory ddeiAuthClientRequestFactory, 
        IJsonConvertWrapper jsonConvertWrapper) : base(jsonConvertWrapper)
    {
        _httpClient = httpClient.ExceptionIfNull();
        _ddeiAuthClientRequestFactory = ddeiAuthClientRequestFactory.ExceptionIfNull();
    }

    public async Task VerifyCmsAuthAsync(DdeiBaseArgDto arg) => await CallDdeiAsync(_ddeiAuthClientRequestFactory.CreateVerifyCmsAuthRequest(arg), arg.CmsAuthValues);

    public async Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiReclassifyDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<DdeiDocumentReclassifiedResponse>(_ddeiAuthClientRequestFactory.CreateReclassifyDocumentRequest(arg), arg.CmsAuthValues);

        return new DocumentReclassifiedResultDto
        {
            DocumentId = response.Id,
            DocumentTypeId = response.DocumentTypeId,
            ReclassificationType = response.ReclassificationType,
            OriginalDocumentTypeId = response.OriginalDocumentTypeId,
            DocumentRenamed = response.DocumentRenamed,
            DocumentRenamedOperationName = response.DocumentRenamedOperationName
        };
    }

    public async Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<DdeiDocumentRenamedResponse>(_ddeiAuthClientRequestFactory.CreateRenameDocumentRequest(arg), arg.CmsAuthValues);

        return new DocumentRenamedResultDto { Id = response.Id, OperationName = response.OperationName };
    }

    protected override HttpClient GetHttpClient(string cmsAuthValues) => _httpClient;
}