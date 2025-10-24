using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers;

public class CaseWitnessStatementMapper : ICaseWitnessStatementMapper
{
    public WitnessStatementDto Map(DdeiCaseWitnessStatementsResponse ddeiResponse)
    {
        return new WitnessStatementDto
        {
            DocumentId = ddeiResponse.DocumentId,
            StatementNumber = ddeiResponse.StatementNumber
        };
    }
        
    public WitnessStatementDto Map(StatementForWitness ddeiResponse)
    {
        return new WitnessStatementDto
        {
            DocumentId = ddeiResponse.DocumentId,
            StatementNumber = ddeiResponse.Title
        };
    }
}