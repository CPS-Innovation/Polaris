namespace Common.Dto.Request
{
    public class ReclassificationExhibit
    {
        public int? ExistingProducerOrWitnessId { get; set; }
        public string NewProducer { get; set; }
        public string Item { get; set; }
        public string Reference { get; set; }
    }
}