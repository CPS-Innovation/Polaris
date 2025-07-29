using System.Text.Json;

namespace shared.integration_tests.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<T?> GetContentResponseAsync<T>(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
    {
        var json = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}