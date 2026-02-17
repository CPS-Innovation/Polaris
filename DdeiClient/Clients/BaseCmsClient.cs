using Common.Exceptions;
using Common.Wrappers;
using System.Net;
using Common.Extensions;
using Common.LayerResponse;
using System.Net.Http;

namespace DdeiClient.Clients;

//BaseCmsClient is used for calling both MDS and DDEI client (now used for auth verification) and passing the cmsAuthValues as part of the request
public abstract class BaseCmsClient
{
    protected readonly IJsonConvertWrapper JsonConvertWrapper;
    protected BaseCmsClient(IJsonConvertWrapper jsonConvertWrapper)
    {
        JsonConvertWrapper = jsonConvertWrapper.ExceptionIfNull();
    }

    protected abstract HttpClient GetHttpClient(string cmsAuthValues);
    protected virtual async Task<T> CallHttpClientAsync<T>(HttpRequestMessage request, string cmsAuthValues)
    {
        using var response = await CallHttpClientAsync(request, cmsAuthValues);
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvertWrapper.DeserializeObject<T>(content);
    }

    //protected virtual async Task<HttpResponseMessage> CallHttpClientAsync(HttpRequestMessage request, string cmsAuthValues, params HttpStatusCode[] expectedUnhappyStatusCodes)
    //{
    //    var httpClient = GetHttpClient(cmsAuthValues);
    //    var response = await httpClient.SendAsync(request);
    //    try
    //    {
    //        if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
    //        {
    //            return response;
    //        }

    //        var content = await response.Content.ReadAsStringAsync();
    //        throw new HttpRequestException(content);
    //    }
    //    catch (HttpRequestException exception)
    //    {
    //        throw new DdeiClientException(response.StatusCode, exception);
    //    }
    //}



    protected virtual async Task<ILayerResponse<HttpResponseMessage>> CallHttpClientAsync(HttpRequestMessage request, string cmsAuthValues, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        var response = new LayerResponse<HttpResponseMessage>();
        var httpResponse = new HttpResponseMessage();
        var httpClient = GetHttpClient(cmsAuthValues);

        try
        {
            httpResponse = await httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            return response.AddError(new HttpError(httpResponse));
        }

        if (httpResponse.IsSuccessStatusCode)
        {
            response.Content = httpResponse;
            return response;
        }

        return response.AddError(new HttpError(httpResponse));
    }
}