using Common.Dto.Response.Case;
using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers
{
    public interface ICaseWitnessMapper
    {
        CaseWitnessDto Map(BaseCaseWitnessResponse ddeiResponse);
    }
}