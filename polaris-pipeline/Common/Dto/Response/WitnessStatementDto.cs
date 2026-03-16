namespace Common.Dto.Response
{
    public class WitnessStatementDto
    {
        public int? DocumentId { get; set; }

        // Do we need this?
        public int? ParentId => DocumentId;
        public int? StatementNumber { get; set; }
    }
}