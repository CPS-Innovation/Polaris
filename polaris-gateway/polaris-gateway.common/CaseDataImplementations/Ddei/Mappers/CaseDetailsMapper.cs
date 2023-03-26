using BusinessDomain = PolarisGateway.Domain.CaseData;
using DDeiDomain = Ddei.Domain;


namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public class CaseDetailsMapper : ICaseDetailsMapper
    {
        private const string NotYetChargedCode = "NYC";

        public BusinessDomain.CaseDetailsFull MapCaseDetails(DDeiDomain.CaseDetails caseDetails)
        {
            var summary = caseDetails.Summary;

            var defendants = MapDefendants(caseDetails);
            var leadDefendant = FindLeadDefendant(defendants, summary);
            var headlineCharge = FindHeadlineCharge(leadDefendant);
            var isCaseCharged = FindIsCaseCharged(defendants);

            return new BusinessDomain.CaseDetailsFull
            {
                Id = summary.Id,
                UniqueReferenceNumber = summary.Urn,
                IsCaseCharged = isCaseCharged,
                NumberOfDefendants = summary.NumberOfDefendants,
                LeadDefendantDetails = leadDefendant.DefendantDetails,
                Defendants = defendants,
                HeadlineCharge = headlineCharge
            };
        }

        private IEnumerable<BusinessDomain.Defendant> MapDefendants(DDeiDomain.CaseDetails caseDetails)
        {
            return caseDetails.Defendants.Select(defendant => MapDefendant(defendant, caseDetails.PreChargeDecisionRequests));
        }

        private BusinessDomain.Defendant MapDefendant(DDeiDomain.CaseDefendant defendant, IEnumerable<DDeiDomain.PreCharge.PcdRequest> pcdRequests)
        {
            return new BusinessDomain.Defendant
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                DefendantDetails = MapDefendantDetails(defendant),
                CustodyTimeLimit = MapCustodyTimeLimit(defendant.CustodyTimeLimit),
                Charges = MapCharges(defendant),
                ProposedCharges = MapProposedCharges(defendant, pcdRequests)
            };
        }

        private BusinessDomain.DefendantDetails MapDefendantDetails(DDeiDomain.CaseDefendant defendant)
        {
            return new BusinessDomain.DefendantDetails
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                FirstNames = defendant.FirstNames,
                Surname = defendant.Surname,
                // todo: no organisation name in DDEI?
                OrganisationName = defendant.Surname,
                Dob = defendant.Dob,
                isYouth = defendant.Youth,
                Type = defendant.Type
            };
        }

        private BusinessDomain.CustodyTimeLimit MapCustodyTimeLimit(DDeiDomain.CustodyTimeLimit custodyTimeLimit)
        {
            return new BusinessDomain.CustodyTimeLimit
            {
                ExpiryDate = custodyTimeLimit.ExpiryDate,
                ExpiryDays = custodyTimeLimit.ExpiryDays,
                ExpiryIndicator = custodyTimeLimit.ExpiryIndicator
            };
        }

        private IEnumerable<BusinessDomain.Charge> MapCharges(DDeiDomain.CaseDefendant defendant)
        {
            var charges = new List<BusinessDomain.Charge>();
            var nextHearingDate = defendant.NextHearing.Date;

            return defendant.Offences
                .Select(offence => MapCharge(offence, nextHearingDate));
        }

        private IEnumerable<BusinessDomain.ProposedCharge> MapProposedCharges(DDeiDomain.CaseDefendant defendant, IEnumerable<DDeiDomain.PreCharge.PcdRequest> pcdRequests)
        {
            return pcdRequests
                      .SelectMany(pcdRequest => pcdRequest.Suspects)
                      // weaknes here:  because we screenscrape, we don't actually ever see a unique numerical id
                      //  for a suspect.  When we want to join between defendants and suspect, all we have are 
                      //  the Dob etc fields to join on, and hope for the best that they all match
                      .Where(suspect => suspect.Dob == defendant.Dob
                                && suspect.FirstNames == defendant.FirstNames
                                && suspect.Surname == defendant.Surname)
                      .SelectMany(suspect => suspect.ProposedCharges)
                      .Select(proposedCharge => MapProposedCharge(proposedCharge));
        }

        private BusinessDomain.Charge MapCharge(DDeiDomain.Offence offence, string nextHearingDate)
        {
            return new BusinessDomain.Charge
            {
                Id = offence.Id,
                ListOrder = offence.ListOrder,
                IsCharged = true, //todo: offences have an Active status in CMS, we probably want to exclude these
                NextHearingDate = nextHearingDate,
                EarlyDate = offence.FromDate,
                LateDate = offence.ToDate,
                Code = offence.Code,
                ShortDescription = offence.Description,
                LongDescription = offence.Description,
                CustodyTimeLimit = MapCustodyTimeLimit(offence.CustodyTimeLimit)
            };
        }

        private BusinessDomain.ProposedCharge MapProposedCharge(DDeiDomain.PreCharge.PcdProposedCharge proposedCharge)
        {
            return new BusinessDomain.ProposedCharge
            {
                Charge = proposedCharge.Charge,
                Date = proposedCharge.Date
            };
        }

        private BusinessDomain.HeadlineCharge MapHeadlineCharge(BusinessDomain.Charge charge)
        {
            return new BusinessDomain.HeadlineCharge
            {
                Charge = charge.LongDescription,
                Date = charge.EarlyDate,
                NextHearingDate = charge.NextHearingDate
            };
        }

        private BusinessDomain.HeadlineCharge MapHeadlineCharge(BusinessDomain.ProposedCharge proposedCharge)
        {
            return new BusinessDomain.HeadlineCharge
            {
                Charge = proposedCharge.Charge,
                Date = proposedCharge.Date
            };
        }
        private BusinessDomain.Defendant FindLeadDefendant(IEnumerable<BusinessDomain.Defendant> defendants, DDeiDomain.CaseSummary caseSummary)
        {

            // todo: this is not ideal, DDEI only gives us the names of the lead defendant, so not 100%
            // that we find the defendant recrod we want (e.g. if there are two John Smiths on the case?) 
            var foundDefendants = defendants.Where(defendant =>
                areStringsEqual(caseSummary.LeadDefendantFirstNames, defendant.DefendantDetails.FirstNames)
                && areStringsEqual(caseSummary.LeadDefendantSurname, defendant.DefendantDetails.Surname)
                && areStringsEqual(caseSummary.LeadDefendantType, defendant.DefendantDetails.Type));

            // todo: needs logging as to which logic was used
            if (foundDefendants.Count() == 1)
            {
                // we have found one and only one defendant based on name and type match
                return foundDefendants.First();
            }
            else
            {
                return defendants
                    .OrderBy(defendant => defendant.ListOrder)
                    .FirstOrDefault();
            }
        }

        private BusinessDomain.HeadlineCharge FindHeadlineCharge(BusinessDomain.Defendant leadDefendant)
        {
            var firstCharge = leadDefendant.Charges
                .OrderBy(charge => charge.ListOrder)
                .FirstOrDefault();

            if (firstCharge != null)
            {
                return MapHeadlineCharge(firstCharge);
            }

            var firstProposedCharge = leadDefendant.ProposedCharges.FirstOrDefault();

            if (firstProposedCharge != null)
            {
                return MapHeadlineCharge(firstProposedCharge);
            }

            // todo: what to do if we have no charges?
            return new BusinessDomain.HeadlineCharge();
        }

        private bool FindIsCaseCharged(IEnumerable<BusinessDomain.Defendant> defendants)
        {
            return defendants
                .SelectMany(defendant => defendant.Charges)
                .Where(charge => charge.Code != NotYetChargedCode)
                .Any();
        }

        private bool areStringsEqual(string a, string b) => (string.IsNullOrEmpty(a)
             && string.IsNullOrEmpty(b))
             || string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
    }
}