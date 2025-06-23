namespace Common.Dto.Request
{
    public record CommunicationExhibitType
    {
        public string Item { get; set; }

        public string Reference { get; set; }

        public int? ExistingProducerOrWitnessId { get; set; }

        public string Producer { get; set; }
    }
}