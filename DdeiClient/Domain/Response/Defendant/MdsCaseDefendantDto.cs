using System.Text.Json.Serialization;

namespace Ddei.Domain.Response.Defendant;

public class MdsCaseDefendantDto
{
    public int Id { get; set; }
    public int? ListOrder { get; set; }
    public string Type { get; set; }
    public string FirstNames { get; set; }
    public string Surname { get; set; }
    public string Dob { get; set; }
    public string RemandStatus { get; set; }
    [JsonPropertyName("policeRemandStatus")]
    public string PoliceRemandStatus { get => RemandStatus; set => RemandStatus = value; }
    public bool Youth { get; set; }
    public DdeiCustodyTimeLimitDto CustodyTimeLimit { get; set; }

    public IEnumerable<MdsOffenceDto> Offences { get; set; }

    public MdsNextHearingDto NextHearing { get; set; }
}