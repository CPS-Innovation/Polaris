using Common.Dto.Case;
using Ddei.Domain;
using Ddei.Domain.PreCharge;
using DdeiClient.Mappers.Contract;

namespace Ddei.Mappers
{
    public class CaseDetailsMapper : ICaseDetailsMapper
    {
        private const string NotYetChargedCode = "NYC";

        public CaseDto MapCaseDetails(DdeiCaseDetailsDto caseDetails)
        {
            var summary = caseDetails.Summary;

            var defendants = MapDefendants(caseDetails);
            var leadDefendant = FindLeadDefendant(defendants, summary);
            var headlineCharge = FindHeadlineCharge(leadDefendant);
            var isCaseCharged = FindIsCaseCharged(defendants);

            return new CaseDto
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

        private IEnumerable<DefendantDto> MapDefendants(DdeiCaseDetailsDto caseDetails)
        {
            return caseDetails.Defendants.Select(defendant => MapDefendant(defendant, caseDetails.PreChargeDecisionRequests));
        }

        private DefendantDto MapDefendant(DdeiCaseDefendantDto defendant, IEnumerable<DdeiPcdRequestDto> pcdRequests)
        {
            return new DefendantDto
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                DefendantDetails = MapDefendantDetails(defendant),
                CustodyTimeLimit = MapCustodyTimeLimit(defendant.CustodyTimeLimit),
                Charges = MapCharges(defendant),
                ProposedCharges = MapProposedCharges(defendant, pcdRequests)
            };
        }

        private DefendantDetails MapDefendantDetails(DdeiCaseDefendantDto defendant)
        {
            return new DefendantDetails
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

        private CustodyTimeLimitDto MapCustodyTimeLimit(DdeiCustodyTimeLimitDto custodyTimeLimit)
        {
            return new CustodyTimeLimitDto
            {
                ExpiryDate = custodyTimeLimit.ExpiryDate,
                ExpiryDays = custodyTimeLimit.ExpiryDays,
                ExpiryIndicator = custodyTimeLimit.ExpiryIndicator
            };
        }

        private IEnumerable<ChargeDto> MapCharges(DdeiCaseDefendantDto defendant)
        {
            var charges = new List<ChargeDto>();
            var nextHearingDate = defendant.NextHearing.Date;

            return defendant.Offences
                .Select(offence => MapCharge(offence, nextHearingDate));
        }

        private IEnumerable<ProposedChargeDto> MapProposedCharges(DdeiCaseDefendantDto defendant, IEnumerable<DdeiPcdRequestDto> pcdRequests)
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

        private ChargeDto MapCharge(DdeiOffenceDto offence, string nextHearingDate)
        {
            return new ChargeDto
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

        private ProposedChargeDto MapProposedCharge(DdeiPcdProposedChargeDto proposedCharge)
        {
            return new ProposedChargeDto
            {
                Charge = proposedCharge.Charge,
                Date = proposedCharge.Date
            };
        }

        private HeadlineChargeDto MapHeadlineCharge(ChargeDto charge)
        {
            return new HeadlineChargeDto
            {
                Charge = charge.LongDescription,
                Date = charge.EarlyDate,
                NextHearingDate = charge.NextHearingDate
            };
        }

        private HeadlineChargeDto MapHeadlineCharge(ProposedChargeDto proposedCharge)
        {
            return new HeadlineChargeDto
            {
                Charge = proposedCharge.Charge,
                Date = proposedCharge.Date
            };
        }
        private DefendantDto FindLeadDefendant(IEnumerable<DefendantDto> defendants, DdeiCaseSummaryDto caseSummary)
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

        private HeadlineChargeDto FindHeadlineCharge(DefendantDto leadDefendant)
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
            return new HeadlineChargeDto();
        }

        private bool FindIsCaseCharged(IEnumerable<DefendantDto> defendants)
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