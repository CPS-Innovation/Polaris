using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class DefendantDetailsDto
    {
        public DefendantDetailsDto()
        {
        }

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonProperty("listOrder")]
        [JsonPropertyName("listOrder")]
        public int? ListOrder { get; set; }

        [JsonProperty("firstNames")]
        [JsonPropertyName("firstNames")]
        public string FirstNames { get; set; }

        [JsonProperty("surname")]
        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        [JsonProperty("organisationName")]
        [JsonPropertyName("organisationName")]
        public string OrganisationName { get; set; }

        [JsonProperty("dob")]
        [JsonPropertyName("dob")]
        public string Dob { get; set; }

        public string Age
        {
            get
            {
                if (IsYyyyMmDd())
                {
                    // https://stackoverflow.com/questions/9/how-do-i-calculate-someones-age-based-on-a-datetime-type-birthday
                    DateTime dob = GetYyyyMmDdDateOfBirth();
                    var today = DateTime.Today;
                    var age = today.Year - dob.Year;
                    if (dob.Date > today.AddYears(-age))
                        age--;

                    return age.ToString();
                }

                return string.Empty;
            }
        }

        private DateTime GetYyyyMmDdDateOfBirth()
        {
            return new DateTime(int.Parse(Dob.Substring(0, 4)), int.Parse(Dob.Substring(5, 2)), int.Parse(Dob.Substring(8, 2)));
        }

        public string GetDdMmYyyyDateOfBirth()
        {
            if (IsYyyyMmDd())
                return $"{Dob.Substring(8, 2)}/{Dob.Substring(5, 2)}/{Dob.Substring(0, 4)}";

            return Dob;
        }

        private bool IsYyyyMmDd()
        {
            return Dob?.Length == 10
                    && char.IsDigit(Dob[0]) && char.IsDigit(Dob[1]) && char.IsDigit(Dob[2]) && char.IsDigit(Dob[3])
                    && Dob[4] == '-'
                    && char.IsDigit(Dob[5]) && char.IsDigit(Dob[6])
                    && Dob[7] == '-'
                    && char.IsDigit(Dob[8]) && char.IsDigit(Dob[9]);
        }

        [JsonProperty("youth")]
        [JsonPropertyName("youth")]
        public bool IsYouth { get; set; }

        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
