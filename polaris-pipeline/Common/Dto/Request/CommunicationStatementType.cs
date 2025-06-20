using System;

namespace Common.Dto.Request
{
    public record CommunicationStatementType
    {
        public int Witness { get; set; }

        public int StatementNo { get; set; }

        public DateOnly Date { get; set; }
    }
}
