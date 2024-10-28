using Common.Dto.Response.Case;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public class CaseWitnessMapper : ICaseWitnessMapper
    {
        public CaseWitnessDto Map(DdeiCaseWitnessResponse ddeiResponse)
        {
            return new CaseWitnessDto
            {
                Id = ddeiResponse.Id,
                Name = ddeiResponse.Name
            };
        }
    }
}