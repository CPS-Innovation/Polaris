namespace Ddei.Domain.Response.Defendant
{
    public class MdsCustodyTimeLimitDto
    {
        public string ExpiryDate { get; set; }
        public int? ExpiryDays { get; set; }
        public string ExpiryIndicator { get; set; }
    }
}