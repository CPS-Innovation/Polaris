namespace coordinator.Clients.Ddei.Domain
{
    public class DdeiCustodyTimeLimit
    {
        public string ExpiryDate { get; set; }
        public int? ExpiryDays { get; set; }
        public string ExpiryIndicator { get; set; }
    }
}