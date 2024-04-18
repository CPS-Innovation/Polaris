using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Azure;
using Common.Exceptions;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common.Handlers
{
    public class ExceptionHandler : IExceptionHandler
    {
        public HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger)
        {
            BuildErrorMessage(exception, out var errorMessage, out var errorCode);

            logger.LogMethodError(correlationId, source, $"{errorMessage}: {exception.Message}", exception);
            logger.LogError(exception, "A {Source} exception has occurred", source);

            return ErrorResponse(errorMessage, exception, errorCode);
        }

        public HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger, object obj)
        {
            BuildErrorMessage(exception, out var errorMessage, out var errorCode);

            logger.LogMethodError(correlationId, source, $"{errorMessage}: {exception.Message}", exception);
            logger.LogError(exception, "A {Source} exception has occurred", source);

            return ErrorResponse(errorMessage, exception, errorCode, obj);
        }

        public ObjectResult HandleExceptionNew(Exception exception, Guid correlationId, string source, ILogger logger)
        {
            BuildErrorMessage(exception, out var errorMessage, out var errorCode);

            logger.LogMethodError(correlationId, source, $"{errorMessage}: {exception.Message}", exception);
            logger.LogError(exception, "A {Source} exception has occurred", source);

            return ErrorResponseNew(errorMessage, exception, errorCode);
        }

        private static void BuildErrorMessage(Exception exception, out string errorMessage, out HttpStatusCode errorCode)
        {
            var baseErrorMessage = "An unhandled exception occurred";
            var statusCode = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case BadRequestException:
                    baseErrorMessage = "Invalid request";
                    statusCode = HttpStatusCode.BadRequest;
                    break;


                //this exception is thrown when generating a sas link
                case RequestFailedException requestFailedException:
                    baseErrorMessage = "A service request failed exception occurred";
                    var requestFailedStatusCode = (HttpStatusCode)requestFailedException.Status;
                    statusCode =
                        requestFailedStatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound
                            ? statusCode
                            : requestFailedStatusCode;
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            errorMessage = baseErrorMessage;
            errorCode = statusCode;
        }

        private static HttpResponseMessage ErrorResponse(string baseErrorMessage, Exception exception, HttpStatusCode httpStatusCode)
        {
            var errorMessage = $"{baseErrorMessage}. Base exception message: {exception.GetBaseException().Message}";
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }

        private static HttpResponseMessage ErrorResponse(string baseErrorMessage, Exception exception, HttpStatusCode httpStatusCode, object obj)
        {
            var errorMessage = $"{baseErrorMessage}. Base exception message: {exception.GetBaseException().Message}";

            var responseContent = new ExceptionContent
            {
                ErrorMessage = errorMessage,
                Data = obj,
                DataType = obj.GetType().Name
            };

            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }

        private static ObjectResult ErrorResponseNew(string baseErrorMessage, Exception exception, HttpStatusCode httpStatusCode)
        {
            var errorMessage = $"{baseErrorMessage}. Base exception message: {exception.GetBaseException().Message}";
            return new ObjectResult(errorMessage)
            {
                StatusCode = (int)httpStatusCode
            };
        }
    }

    public class ExceptionContent
    {
        public string ErrorMessage { get; set; }
        public object Data { get; set; }
        public string DataType { get; set; }
    }
}
