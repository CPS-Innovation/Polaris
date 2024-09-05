namespace Common.Dto.Request
{
    public class ReclassificationStatement
    {
        public int WitnessId { get; set; }
        public int StatementNo { get; set; }
        public string Date { get; set; }
        public bool Used { get; set; }
    }
}