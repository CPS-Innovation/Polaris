using Common.Dto.Response;

namespace Ddei.Mappers
{
    public interface ICaseWitnessStatementMapper
    {
        WitnessStatementDto Map(DdeiCaseWitnessStatementsResponse ddeiResponse);
    }
}