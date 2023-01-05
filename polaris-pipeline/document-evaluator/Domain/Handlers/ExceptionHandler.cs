using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Azure;
using Common.Domain.Exceptions;
using Common.Exceptions;
using Common.Exceptions.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace document_evaluator.Domain.Handlers
{
    public class ExceptionHandler : IExceptionHandler
    {
        public HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger)
        {
            logger.LogMethodEntry(correlationId, nameof(ExceptionHandler), string.Empty);
            var baseErrorMessage = "An unhandled exception occurred";
            var statusCode = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case UnauthorizedException:
                    baseErrorMessage = "Unauthorized";
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                case BadRequestException:
                    baseErrorMessage = "Invalid request";
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case HttpException httpException:
                    baseErrorMessage = "An http exception occurred";
                    statusCode =
                        httpException.StatusCode == HttpStatusCode.BadRequest
                            ? statusCode
                            : httpException.StatusCode;
                    break;
                case RequestFailedException requestFailedException:
                {
                    baseErrorMessage = "A service request failed exception occurred";
                    var requestFailedStatusCode = (HttpStatusCode)requestFailedException.Status;
                    statusCode =
                        requestFailedStatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound
                            ? statusCode
                            : requestFailedStatusCode;
                    break;
                }
            }

            logger.LogMethodError(correlationId, source, $"{baseErrorMessage}: {exception.Message}", exception);
            logger.LogMethodExit(correlationId, nameof(ExceptionHandler), string.Empty);
            return ErrorResponse(baseErrorMessage, exception, statusCode);
        }

        private static HttpResponseMessage ErrorResponse(string baseErrorMessage, Exception exception, HttpStatusCode httpStatusCode)
        {
            var errorMessage = $"{baseErrorMessage}. Base exception message: {exception.GetBaseException().Message}";
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }
    }
}
