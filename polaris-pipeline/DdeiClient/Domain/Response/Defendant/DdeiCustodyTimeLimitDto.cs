namespace Ddei.Domain.Response.Defendant
{
    public class DdeiCustodyTimeLimitDto
    {
        public string ExpiryDate { get; set; }
        public int? ExpiryDays { get; set; }
        public string ExpiryIndicator { get; set; }
    }
}