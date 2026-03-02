using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers;

public interface ICaseWitnessStatementMapper
{
    //WitnessStatementDto Map(DdeiCaseWitnessStatementsResponse ddeiResponse);
    //  Verify if this may be needed in the future
    WitnessStatementDto Map(StatementForWitness ddeiResponse);
}