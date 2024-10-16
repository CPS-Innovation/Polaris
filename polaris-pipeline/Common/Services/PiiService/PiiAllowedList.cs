using System.Collections.Generic;

namespace Common.Services.PiiService
{
    public class PiiAllowedList : IPiiAllowedList
    {
        private const string PersonType = "PersonType";

        public List<PiiAllowedWord> GetWords()
        {
            return new()
            {
                new("colleague",    PiiCategory.PersonType),
                new("customer",     PiiCategory.PersonType),
                new("customers",    PiiCategory.PersonType),
                new("defendent",    PiiCategory.PersonType),
                new("interviewer",  PiiCategory.PersonType),
                new("officer",      PiiCategory.PersonType),
                new("officers",     PiiCategory.PersonType),
                new("police",       PiiCategory.PersonType),
                new("police",       PiiCategory.Organization),
                new("prosecutor",   PiiCategory.PersonType),
                new("victim",       PiiCategory.PersonType),
                new("witness",      PiiCategory.PersonType)
            };
        }
    }
}