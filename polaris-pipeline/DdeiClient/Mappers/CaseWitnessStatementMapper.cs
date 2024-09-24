using Common.Dto.Response;

namespace Ddei.Mappers
{
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
    }
}