using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Domain.QueueItems;
using Common.Domain.Requests;
using Common.Handlers;
using Common.Logging;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace document_evaluator.Functions
{
    public class ProcessEvaluatedDocuments
    {
        private readonly IAuthorizationValidator _authorizationValidator;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<ProcessDocumentsToRemoveRequest> _validatorWrapper;
        private readonly ILogger<ProcessEvaluatedDocuments> _log;
        private readonly IConfiguration _configuration;
        private readonly IStorageQueueService _storageQueueService;

        public ProcessEvaluatedDocuments(IAuthorizationValidator authorizationValidator, ILogger<ProcessEvaluatedDocuments> logger, IJsonConvertWrapper jsonConvertWrapper, 
            IConfiguration configuration, IValidatorWrapper<ProcessDocumentsToRemoveRequest> validatorWrapper, IStorageQueueService storageQueueService)
        {
           _log = logger;
           _authorizationValidator = authorizationValidator;
           _jsonConvertWrapper = jsonConvertWrapper;
           _configuration = configuration;
           _validatorWrapper = validatorWrapper;
           _storageQueueService = storageQueueService;
        }
        
        [FunctionName("process-evaluated-documents")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "processEvaluatedDocuments")] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "ProcessEvaluatedDocuments - Run";
            
            try
            {
                request.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

                var correlationId = correlationIdValues.First();
                if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                        throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);
                
                _log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                var authValidation = await _authorizationValidator.ValidateTokenAsync(request.Headers.Authorization, currentCorrelationId, 
                    PipelineScopes.ProcessEvaluatedDocuments, PipelineRoles.EmptyRole);
                if (!authValidation.Item1)
                    throw new UnauthorizedException("Token validation failed");

                if (request.Content == null)
                    throw new BadRequestException("Request body has no content", nameof(request));

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new BadRequestException("Request body cannot be null.", nameof(request));
                
                var processEvaluatedDocumentsRequest = _jsonConvertWrapper.DeserializeObject<ProcessDocumentsToRemoveRequest>(content);
                if (processEvaluatedDocumentsRequest == null)
                    throw new Exception($"An invalid message was received '{content}'");

                var results = _validatorWrapper.Validate(processEvaluatedDocumentsRequest);
                if (results.Any())
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                
                if (processEvaluatedDocumentsRequest.DocumentsToRemove is {Count: > 0})
                {
                    foreach (var payload in processEvaluatedDocumentsRequest.DocumentsToRemove)
                    {
                        await _storageQueueService.AddNewMessageAsync(
                            _jsonConvertWrapper.SerializeObject(new UpdateSearchIndexByVersionQueueItem(processEvaluatedDocumentsRequest.CaseId, payload.DocumentId, 
                                payload.VersionId, currentCorrelationId)), _configuration[ConfigKeys.SharedKeys.UpdateSearchIndexByVersionQueueName]);
                    }
                }
            }
            catch (Exception exception)
            {
                _log.LogMethodError(currentCorrelationId, loggingName, exception.Message, exception);
            }
            finally
            {
                _log.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
            }
            return new HttpResponseMessage(HttpStatusCode.OK); //log any errors and allow an "OK" regardless so that the pipeline process all documents it should
        }
    }
}