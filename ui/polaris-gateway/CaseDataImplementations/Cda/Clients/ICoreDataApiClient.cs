using System;
using RumpoleGateway.CaseDataImplementations.Cda.Domain.CaseDetails;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RumpoleGateway.CaseDataImplementations.Cda.Clients
{
    public interface ICoreDataApiClient
    {
        Task<CaseDetails> GetCaseDetailsByIdAsync(string caseId, string accessToken, Guid correlationId);

        Task<IList<CaseDetails>> GetCaseInformationByUrnAsync(string urn, string accessToken, Guid correlationId);
    }
}
