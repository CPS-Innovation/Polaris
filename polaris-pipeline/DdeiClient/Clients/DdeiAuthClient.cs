using Common.Extensions;
using Common.Wrappers;
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

    protected override HttpClient GetHttpClient(string cmsAuthValues) => _httpClient;
}