using Common.Dto.Response.Case;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public interface ICaseWitnessMapper
    {
        CaseWitnessDto Map(DdeiCaseWitnessResponse ddeiResponse);
    }
}