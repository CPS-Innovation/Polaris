using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using PolarisGateway.CaseDataImplementations.Cda.Domain.ResponseTypes;
using PolarisGateway.CaseDataImplementations.Cda.Domain.CaseDetails;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Extensions;
using PolarisGateway.CaseDataImplementations.Cda.Factories;
using PolarisGateway.CaseDataImplementations.Cda.Exceptions;

namespace PolarisGateway.CaseDataImplementations.Cda.Clients
{
    public class CoreDataApiClient : ICoreDataApiClient
    {
        private readonly IGraphQLClient _coreDataApiClient;
        private readonly IAuthenticatedGraphQLHttpRequestFactory _authenticatedGraphQLHttpRequestFactory;
        private readonly ILogger<CoreDataApiClient> _logger;
        public CoreDataApiClient(IGraphQLClient coreDataApiClient, IAuthenticatedGraphQLHttpRequestFactory authenticatedGraphQLHttpRequestFactory,
            ILogger<CoreDataApiClient> logger)
        {
            _coreDataApiClient = coreDataApiClient;
            _authenticatedGraphQLHttpRequestFactory = authenticatedGraphQLHttpRequestFactory;
            _logger = logger;
        }

        public async Task<CaseDetails> GetCaseDetailsByIdAsync(string caseId, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetCaseDetailsByIdAsync), $"For CaseId: {caseId}");
            CaseDetails caseDetails = null;

            try
            {
                var query = new GraphQLHttpRequest
                {
                    Query = "query {case(id: " + caseId + ")  {id uniqueReferenceNumber caseType  appealType caseStatus {code description } "
                            + " leadDefendant {firstNames surname organisationName}  "
                            + " offences { earlyDate lateDate listOrder code shortDescription longDescription }  }}"
                };

                var authenticatedRequest = _authenticatedGraphQLHttpRequestFactory.Create(accessToken, query, correlationId);

                _logger.LogMethodFlow(correlationId, nameof(GetCaseDetailsByIdAsync), $"Sending the following query to the Core Data API: {query.ToJson()}");
                var response = await _coreDataApiClient.SendQueryAsync<ResponseCaseDetails>(authenticatedRequest);

                if (response.Data?.CaseDetails == null) return null;

                caseDetails = response.Data.CaseDetails;
                return caseDetails;
            }
            catch (Exception exception)
            {
                throw new CoreDataApiException("Error response from Core Data Api.", exception);
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(GetCaseDetailsByIdAsync), caseDetails.ToJson());
            }
        }

        public async Task<IList<CaseDetails>> GetCaseInformationByUrnAsync(string urn, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetCaseInformationByUrnAsync), $"For Urn: {urn}");
            IList<CaseDetails> caseDetailsCollection = null;
            try
            {
                var query = new GraphQLHttpRequest
                {
                    Query = "query {cases(urn: \"" + urn + "\")  "
                            + " {id uniqueReferenceNumber caseType  appealType caseStatus {code description } "
                            + " leadDefendant {firstNames surname organisationName}"
                            + " offences { earlyDate lateDate listOrder code shortDescription longDescription }  }}"
                };

                var authenticatedRequest = _authenticatedGraphQLHttpRequestFactory.Create(accessToken, query, correlationId);

                _logger.LogMethodFlow(correlationId, nameof(GetCaseDetailsByIdAsync), $"Sending the following query to the Core Data API: {query.ToJson()}");
                var response = await _coreDataApiClient.SendQueryAsync<ResponseCaseInformationByUrn>(authenticatedRequest);

                if (response.Data == null || response.Data?.CaseDetails?.Count == 0)
                {
                    if (response.Errors == null)
                        return null;

                    var sb = new StringBuilder();
                    foreach (var error in response.Errors)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");

                        sb.Append(error.Message);
                    }

                    throw new Exception($"Error response from the Core Data Api. {sb}");
                }

                if (response.Data != null) caseDetailsCollection = response.Data.CaseDetails;
                return caseDetailsCollection;
            }
            catch (Exception exception)
            {
                throw new CoreDataApiException("Error response from the Core Data Api.", exception);
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(GetCaseInformationByUrnAsync), caseDetailsCollection.ToJson());
            }
        }
    }
}
