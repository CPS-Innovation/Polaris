using System;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;

namespace Common.Clients
{
    public interface IDocumentExtractionClient
	{
		Task<CmsCase> GetCaseDocumentsAsync(string caseId, string accessToken, Guid correlationId);
	}
}

