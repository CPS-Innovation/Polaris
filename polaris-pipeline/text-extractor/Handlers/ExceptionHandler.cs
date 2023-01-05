using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Azure;
using Common.Domain.Exceptions;
using Common.Exceptions.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;
using text_extractor.Domain.Exceptions;

namespace text_extractor.Handlers
{
    public class ExceptionHandler : IExceptionHandler
    {
        public HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger)
        {
            var baseErrorMessage = "An unhandled exception occurred";
            var statusCode = HttpStatusCode.InternalServerError;

            if (exception is UnauthorizedException)
            {
                baseErrorMessage = "Unauthorized";
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (exception is BadRequestException)
            {
                baseErrorMessage = "Invalid request";
                statusCode = HttpStatusCode.BadRequest;
            }
            //this exception is thrown when generating a sas link
            else if (exception is RequestFailedException requestFailedException)
            {
                baseErrorMessage = "A service request failed exception occurred";
                var requestFailedStatusCode = (HttpStatusCode)requestFailedException.Status;
                statusCode =
                    requestFailedStatusCode == HttpStatusCode.BadRequest || requestFailedStatusCode == HttpStatusCode.NotFound
                    ? statusCode
                    : requestFailedStatusCode;
            }
            else if (exception is OcrServiceException)
            {
                baseErrorMessage = "An Ocr service exception occurred";
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
