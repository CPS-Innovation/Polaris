using Newtonsoft.Json;

namespace Common.Dto.Case
{
    public class DefendantDetailsDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("listOrder")]
        public int? ListOrder { get; set; }

        [JsonProperty("firstNames")]
        public string FirstNames { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("organisationName")]
        public string OrganisationName { get; set; }

        [JsonProperty("dob")]
        public string Dob { get; set; }

        [JsonProperty("youth")]
        public bool isYouth { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
