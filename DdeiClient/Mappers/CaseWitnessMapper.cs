using Common.Dto.Response.Case;
using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers
{
    public class CaseWitnessMapper : ICaseWitnessMapper
    {
        public CaseWitnessDto Map(BaseCaseWitnessResponse ddeiResponse)
        {
            return new CaseWitnessDto
            {
                Id = ddeiResponse.Id,
                Name = ddeiResponse.Name
            };
        }
    }
}