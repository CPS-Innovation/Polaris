using Common.Exceptions;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Ddei.Factories.Contracts;
using DdeiClient.Clients.Interfaces;
using System.Net;

namespace DdeiClient.Clients;

public class DdeiAuthClient : IDdeiAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly IDdeiAuthClientRequestFactory _ddeiAuthClientRequestFactory;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    public DdeiAuthClient(
        HttpClient httpClient,
        IDdeiAuthClientRequestFactory ddeiAuthClientRequestFactory, 
        IJsonConvertWrapper jsonConvertWrapper)
    {
        _httpClient = httpClient;
        _ddeiAuthClientRequestFactory = ddeiAuthClientRequestFactory;
        _jsonConvertWrapper = jsonConvertWrapper;
    }

    public async Task VerifyCmsAuthAsync(DdeiBaseArgDto arg) => await CallDdeiAsync(_ddeiAuthClientRequestFactory.CreateVerifyCmsAuthRequest(arg));

    private async Task<HttpResponseMessage> CallDdeiAsync(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        
        var response = await _httpClient.SendAsync(request);
        try
        {
            if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
            {
                return response;
            }

            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(content);
        }
        catch (HttpRequestException exception)
        {
            throw new DdeiClientException(response.StatusCode, exception);
        }
    }
}