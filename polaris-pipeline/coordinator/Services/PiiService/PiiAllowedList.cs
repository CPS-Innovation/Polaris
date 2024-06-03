using System.Collections.Generic;

namespace coordinator.Services.PiiService
{
    public class PiiAllowedList : IPiiAllowedList
    {
        public List<PiiAllowedWord> GetWords()
        {
            return new()
            {
                new("colleague",    "PersonType"),
                new("customer",     "PersonType"),
                new("customers",    "PersonType"),
                new("defendent",    "PersonType"),
                new("interviewer",  "PersonType"),
                new("officer",      "PersonType"),
                new("officers",     "PersonType"),
                new("police",       "PersonType"),
                new("police",       "Organization"),
                new("prosecutor",   "PersonType"),
                new("victim",       "PersonType"),
                new("witness",      "PersonType")
            };
        }
    }
}