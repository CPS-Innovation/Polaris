using System.Collections.Generic;

namespace Common.Dto.Case
{
    public class OrderedStatementsDto
    {
        public OrderedStatementsDto()
        {
            Statements = new List<OrderedStatementDto>();
        }

        public List<OrderedStatementDto> Statements { get; set; }
    }
}