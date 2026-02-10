using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers;

public interface ICaseWitnessStatementMapper
{
    WitnessStatementDto Map(DdeiCaseWitnessStatementsResponse ddeiResponse);
    WitnessStatementDto Map(StatementForWitness ddeiResponse);
}