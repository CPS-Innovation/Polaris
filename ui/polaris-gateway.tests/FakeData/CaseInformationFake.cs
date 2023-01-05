using System.Collections.Generic;
using RumpoleGateway.Domain.CaseData;

namespace RumpoleGateway.Tests.FakeData
{
    public class CaseInformationFake
    {
        public IList<CaseDetails> GetCaseInformationByURN_Payload()
        {
            var lstCaseDetails = new List<CaseDetails>
            {
                // new CaseDetails { Id = 18868,
                // UniqueReferenceNumber = "10OF1234520",
                // CaseType = "O",
                // AppealType = "",
                // CaseStatus = new CaseStatus{Code ="LV",
                //                             Description = "Live Case" }  ,
                // LeadDefendantDetails =   new DefendantDetails {FirstNames = "Connor",
                //                                      Surname="Rich",
                //                                      OrganisationName =""},
                // Offences = new List<Charge> { new Charge {EarlyDate= "2021-12-08",
                //                                             LateDate = "2021-12-10",
                //                                             ListOrder = 1,
                //                                             Code ="MDR",
                //                                             ShortDescription="Short description for URN 10OF1234520 offence 1",
                //                                             LongDescription="This is a very long description for URN 10OF1234520 offence 1"} } } 
            };


            return lstCaseDetails;
        }
    }
}
