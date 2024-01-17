using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Azure;
using Common.Domain.Exceptions;
using Common.Exceptions;
using Common.Handlers.Contracts;
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
                case UnauthorizedException:
                    baseErrorMessage = "Unauthorized";
                    statusCode = HttpStatusCode.Unauthorized;
                    break;

                case BadRequestException or UnsupportedFileTypeException:
                    baseErrorMessage = "Invalid request";
                    statusCode = HttpStatusCode.BadRequest;
                    break;

                case DdeiClientException httpException:
                    baseErrorMessage = "An DDEI client exception occurred";
                    statusCode =
                        httpException.StatusCode == HttpStatusCode.BadRequest
                            ? statusCode
                            : httpException.StatusCode;
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

                case OcrServiceException:
                    baseErrorMessage = "An Ocr service exception occurred";
                    break;

                case PdfConversionException:
                    statusCode = HttpStatusCode.NotImplemented;
                    baseErrorMessage = "A failed to convert to pdf exception occurred";
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
        
        private static ObjectResult ErrorResponseNew(string baseErrorMessage, Exception exception, HttpStatusCode httpStatusCode)
        {
            var errorMessage = $"{baseErrorMessage}. Base exception message: {exception.GetBaseException().Message}";
            return new ObjectResult(new StringContent(errorMessage, Encoding.UTF8, MediaTypeNames.Application.Json))
            {
                StatusCode = (int)httpStatusCode
            };
        }
    }
}
