using System.Net;
namespace PolarisGateway.Domain.CaseData.Args
{
    public class UrnArg : BaseCaseDataArg
    {
        public string Urn { get; set; }

        public string UrlSafeUrn
        {
            get
            {
                return WebUtility.UrlEncode(Urn);
            }
        }
    }
}