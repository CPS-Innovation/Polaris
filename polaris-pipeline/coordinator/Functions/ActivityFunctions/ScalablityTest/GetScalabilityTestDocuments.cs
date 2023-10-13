#if SCALABILITY_TEST
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Document;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.ActivityFunctions.Case
{
    public class GetScalabilityTestDocuments
    {
        [FunctionName(nameof(GetScalabilityTestDocuments))]
        public async Task<CmsDocumentDto[]> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<GetScalabilityTestDocumentsActivityPayload>();

            Fixture fixture = new Fixture();

            var cmsDocuments = fixture.CreateMany<CmsDocumentDto>(payload.DocumentCount);
            for(var i=0; i < cmsDocuments.Count(); i++)
            {
                cmsDocuments.ElementAt(i).DocumentId = $"DOC-{i+1}";
            }

            return await Task.FromResult(cmsDocuments.ToArray());
        }
    }
}
#endif
