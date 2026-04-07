using System.Net;

namespace polaris_gateway.integration_tests.ApiClients;

public sealed class ApiClientFileResponse
{
    public ApiClientFileResponse(HttpStatusCode statusCode, byte[] bytes, string? contentType, string? fileName)
    {
        HttpStatusCode = statusCode;
        Bytes = bytes ?? Array.Empty<byte>();
        ContentType = contentType;
        FileName = fileName;
    }

    public HttpStatusCode HttpStatusCode { get; }

    public byte[] Bytes { get; }

    public string? ContentType { get; }

    public string? FileName { get; }
}