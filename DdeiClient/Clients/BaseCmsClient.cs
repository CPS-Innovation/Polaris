using System.Net;
using System.Text.Json;
using Common.Exceptions;
using Common.Extensions;
using Common.Wrappers;

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
        using var response = await CallHttpClientAsync(request, cmsAuthValues, System.Threading.CancellationToken.None);
        var content = await response.Content.ReadAsStringAsync();

        try
        {
            return this.JsonConvertWrapper.DeserializeObject<T>(content);
        }
        catch (JsonException ex)
        {
            // CMS service returned invalid or incompatible JSON
            throw new HttpRequestException(
                "Invalid JSON received from cms.",
                ex,
                HttpStatusCode.BadGateway);
        }
    }

    protected virtual async Task<T> CallHttpClientAsync<T>(HttpRequestMessage request, string cmsAuthValues, System.Threading.CancellationToken cancellationToken)
    {
        using var response = await CallHttpClientAsync(request, cmsAuthValues, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            return this.JsonConvertWrapper.DeserializeObject<T>(content);
        }
        catch (JsonException ex)
        {
            // CMS service returned invalid or incompatible JSON
            throw new HttpRequestException(
                "Invalid JSON received from cms.",
                ex,
                HttpStatusCode.BadGateway);
        }
    }

    protected virtual async Task<HttpResponseMessage> CallHttpClientAsync(HttpRequestMessage request, string cmsAuthValues, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        return await CallHttpClientAsync(request, cmsAuthValues, System.Threading.CancellationToken.None, expectedUnhappyStatusCodes);
    }

    protected virtual async Task<HttpResponseMessage> CallHttpClientAsync(HttpRequestMessage request, string cmsAuthValues, System.Threading.CancellationToken cancellationToken, params HttpStatusCode[] expectedUnhappyStatusCodes)
    {
        var httpClient = GetHttpClient(cmsAuthValues);
        var response = await httpClient.SendAsync(request, cancellationToken);
        try
        {
            if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
            {
                return response;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(content);
        }
        catch (HttpRequestException exception)
        {
            throw new DdeiClientException(response.StatusCode, exception);
        }
    }
}