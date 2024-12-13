using System;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class DefendantDetailsDto
    {
        public DefendantDetailsDto()
        {
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("listOrder")]
        public int? ListOrder { get; set; }

        [JsonPropertyName("firstNames")]
        public string FirstNames { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        [JsonPropertyName("organisationName")]
        public string OrganisationName { get; set; }

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

        private DateTime GetYyyyMmDdDateOfBirth() =>
            new(int.Parse(Dob[..4]), int.Parse(Dob.Substring(5, 2)), int.Parse(Dob.Substring(8, 2)));

        public string GetDdMmYyyyDateOfBirth()
        {
            if (IsYyyyMmDd())
                return $"{Dob.Substring(8, 2)}/{Dob.Substring(5, 2)}/{Dob[..4]}";

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

        [JsonPropertyName("youth")]
        public bool IsYouth { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
