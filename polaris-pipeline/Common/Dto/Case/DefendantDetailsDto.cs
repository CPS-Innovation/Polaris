using Newtonsoft.Json;
using System;

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

        public string Age
        {
            get
            {
                // YYYY-MM-DD ?
                if((Dob.Length == 10) 
                    && char.IsDigit(Dob[0]) && char.IsDigit(Dob[1]) && char.IsDigit(Dob[2]) && char.IsDigit(Dob[3])
                    && Dob[4] == '-'
                    && char.IsDigit(Dob[5]) && char.IsDigit(Dob[6])
                    && Dob[7] == '-'
                    && char.IsDigit(Dob[8]) && char.IsDigit(Dob[9])
                  )
                {
                    // https://stackoverflow.com/questions/9/how-do-i-calculate-someones-age-based-on-a-datetime-type-birthday
                    var today = DateTime.Today;
                    DateTime dob = new DateTime(int.Parse(Dob.Substring(0, 4)), int.Parse(Dob.Substring(5, 2)), int.Parse(Dob.Substring(8, 2)));
                    var age = today.Year - dob.Year;
                    if (dob.Date > today.AddYears(-age)) age--;

                    return age.ToString();
                }

                return string.Empty;
            }
        }

        [JsonProperty("youth")]
        public bool isYouth { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
