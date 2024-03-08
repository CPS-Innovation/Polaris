using System.Net;

namespace DdeiClient.Exceptions
{
    public class DdeiClientException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public DdeiClientException(HttpStatusCode statusCode, HttpRequestException httpRequestException)
            : base($"The HTTP request failed with status code {statusCode}", httpRequestException)
        {
            StatusCode = statusCode;
        }
    }
}
