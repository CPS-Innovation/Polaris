using Common.Exceptions;
using Common.Wrappers;
using System.Net;
using Common.Extensions;
using System.Text.Json;
using System.Threading;

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
    protected virtual async Task<T> CallHttpClientAsync<T>(HttpRequestMessage request, string cmsAuthValues, CancellationToken cancellationToken = default)
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

    protected virtual async Task<HttpResponseMessage> CallHttpClientAsync(HttpRequestMessage request, string cmsAuthValues, CancellationToken cancellationToken = default, params HttpStatusCode[] expectedUnhappyStatusCodes)
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