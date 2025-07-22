using System.Text.Json;

namespace shared.integration_tests.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<T?> GetContentResponseAsync<T>(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.Deserialize<T>(await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken));
    }
}