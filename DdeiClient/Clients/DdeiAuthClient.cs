using Common.Dto.Response;
using Common.Extensions;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;

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

    public virtual async Task<FileResult> GetDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallDdeiAsync(_ddeiAuthClientRequestFactory.CreateGetDocumentRequest(arg), arg.CmsAuthValues);
        var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

        return new FileResult
        {
            Stream = await response.Content.ReadAsStreamAsync(),
            FileName = fileName
        };
    }

    protected override HttpClient GetHttpClient(string cmsAuthValues) => _httpClient;
}