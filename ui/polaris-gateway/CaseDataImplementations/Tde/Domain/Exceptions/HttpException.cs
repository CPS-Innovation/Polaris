
using System;
using System.Net;
using System.Net.Http;

namespace RumpoleGateway.CaseDataImplementations.Tde.Domain.Exceptions
{
    public class TdeClientException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public TdeClientException(HttpStatusCode statusCode, HttpRequestException httpRequestException)
            : base($"The HTTP request failed with status code {statusCode}", httpRequestException)
        {
            StatusCode = statusCode;
        }
    }
}
