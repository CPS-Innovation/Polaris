using Common.Exceptions;
using Common.Extensions;
using Common.Wrappers;
using Ddei.Domain.Response.Document;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Factories;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DdeiClient.Clients;

public abstract class BaseMdsClient
{
    protected readonly IJsonConvertWrapper JsonConvertWrapper;
    protected readonly HttpClient HttpClient;
    protected BaseMdsClient(IJsonConvertWrapper jsonConvertWrapper, HttpClient httpClient)
    {
        JsonConvertWrapper = jsonConvertWrapper;
        HttpClient = httpClient;
    }

    protected virtual async Task<T> CallDdeiAsync<T>(HttpRequestMessage request, string cmsAuthValues)
    {
        using var response = await CallDdeiAsync(request, cmsAuthValues);
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvertWrapper.DeserializeObject<T>(content);
    }

    protected virtual async Task<HttpResponseMessage> CallDdeiAsync(HttpRequestMessage request, string cmsAuthValues, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        var response = await HttpClient.SendAsync(request);
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