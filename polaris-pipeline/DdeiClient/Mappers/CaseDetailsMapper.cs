using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Ddei.Domain.Response;
using Ddei.Domain.Response.PreCharge;
using Ddei.Domain.Response.Defendant;

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
            var witnesses = MapWitnesses(caseDetails);
            var headlineCharge = FindHeadlineCharge(leadDefendant);
            var isCaseCharged = FindIsCaseCharged(defendants);
            var preChargeDecisionRequests = MapPreChargeDecisionRequests(caseDetails.PreChargeDecisionRequests);

            return new CaseDto
            {
                Id = summary.Id,
                UniqueReferenceNumber = summary.Urn,
                IsCaseCharged = isCaseCharged,
                NumberOfDefendants = summary.NumberOfDefendants,
                OwningUnit = summary.OwningUnit,
                LeadDefendantDetails = leadDefendant != null ? leadDefendant.DefendantDetails : null,
                DefendantsAndCharges = defendants,
                HeadlineCharge = headlineCharge,
                PreChargeDecisionRequests = preChargeDecisionRequests,
                Witnesses = witnesses
            };
        }

        public IEnumerable<DefendantAndChargesDto> MapDefendantsAndCharges(IEnumerable<DdeiCaseDefendantDto> defendants)
        {
            return defendants.Select(defendant => MapDefendantAndCharges(defendant));
        }

        public PcdRequestDto MapPreChargeDecisionRequest(DdeiPcdRequestDto pcdr)
        {
            return new PcdRequestDto
            {
                Id = pcdr.Id,
                DecisionRequiredBy = pcdr.DecisionRequiredBy,
                DecisionRequested = pcdr.DecisionRequested,
                CaseOutline = pcdr.CaseOutline.Select(ol => MapPcdCaseOutlineLine(ol)).ToList(),
                Comments = MapPreChargeDecisionComments(pcdr.Comments),
                Suspects = pcdr.Suspects.Select(s => MapPcdSuspect(s)).ToList()
            };
        }

        public IEnumerable<PcdRequestCoreDto> MapCorePreChargeDecisionRequests(IEnumerable<DdeiPcdRequestCoreDto> pcdRequests)
        {
            return pcdRequests.Select(pcd => MapCorePreChargeDecisionRequest(pcd));
        }

        private PcdRequestCoreDto MapCorePreChargeDecisionRequest(DdeiPcdRequestCoreDto pcd)
        {
            return new PcdRequestCoreDto
            {
                Id = pcd.Id,
                DecisionRequiredBy = pcd.DecisionRequiredBy,
                DecisionRequested = pcd.DecisionRequested,
            };
        }

        private DefendantAndChargesDto MapDefendantAndCharges(DdeiCaseDefendantDto defendant)
        {
            return new DefendantAndChargesDto
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                DefendantDetails = MapDefendantDetails(defendant),
                CustodyTimeLimit = MapCustodyTimeLimit(defendant.CustodyTimeLimit),
                Charges = MapCharges(defendant),
            };
        }

        private IEnumerable<DefendantAndChargesDto> MapDefendants(DdeiCaseDetailsDto caseDetails)
        {
            return caseDetails.Defendants.Select(defendant => MapDefendant(defendant, caseDetails.PreChargeDecisionRequests));
        }

        private DefendantAndChargesDto MapDefendant(DdeiCaseDefendantDto defendant, IEnumerable<DdeiPcdRequestDto> pcdRequests)
        {
            return new DefendantAndChargesDto
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                DefendantDetails = MapDefendantDetails(defendant),
                CustodyTimeLimit = MapCustodyTimeLimit(defendant.CustodyTimeLimit),
                Charges = MapCharges(defendant),
                ProposedCharges = MapProposedCharges(defendant, pcdRequests)
            };
        }

        private DefendantDetailsDto MapDefendantDetails(DdeiCaseDefendantDto defendant)
        {
            return new DefendantDetailsDto
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                FirstNames = defendant.FirstNames,
                Surname = defendant.Surname,
                // todo: no organisation name in DDEI?
                OrganisationName = defendant.Surname,
                Dob = defendant.Dob,
                IsYouth = defendant.Youth,
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

        private IEnumerable<WitnessDto> MapWitnesses(DdeiCaseDetailsDto caseDetails)
        {
            return caseDetails.Witnesses.Select(witness => MapWitness(witness));
        }

        private WitnessDto MapWitness(DdeiWitnessDto witness)
        {
            return new WitnessDto
            {
                Id = witness.Id,
                ShoulderNumber = witness.ShoulderNumber,
                Title = witness.Title,
                Name = witness.Name,
                HasStatements = witness.HasStatements,
                ListOrder = witness.ListOrder,
                Child = witness.Child,
                Expert = witness.Expert,
                GreatestNeed = witness.GreatestNeed,
                Prisoner = witness.Prisoner,
                Interpreter = witness.Interpreter,
                Vulnerable = witness.Vulnerable,
                Police = witness.Police,
                Professional = witness.Professional,
                SpecialNeeds = witness.SpecialNeeds,
                Intimidated = witness.Intimidated,
                Victim = witness.Victim
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
                      // weakness here:  because we screen-scrape, we don't actually ever see a unique numerical id
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
                EarlyDate = proposedCharge.EarlyDate,
                LateDate = proposedCharge.LateDate
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
                EarlyDate = proposedCharge.EarlyDate,
                LateDate = proposedCharge.LateDate
            };
        }
        private DefendantAndChargesDto FindLeadDefendant(IEnumerable<DefendantAndChargesDto> defendants, DdeiCaseSummaryDto caseSummary)
        {

            // todo: this is not ideal, DDEI only gives us the names of the lead defendant, so not 100%
            // that we find the defendant recrod we want (e.g. if there are two John Smiths on the case?) 
            var foundDefendants = defendants.Where(defendant =>
                AreStringsEqual(caseSummary.LeadDefendantFirstNames, defendant.DefendantDetails.FirstNames)
                && AreStringsEqual(caseSummary.LeadDefendantSurname, defendant.DefendantDetails.Surname)
                && AreStringsEqual(caseSummary.LeadDefendantType, defendant.DefendantDetails.Type));

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

        private HeadlineChargeDto FindHeadlineCharge(DefendantAndChargesDto leadDefendant)
        {
            if (leadDefendant == null)
            {
                // #24083 - ddei sometimes returns no defendants, so no lead defendant exists.
                return new HeadlineChargeDto();
            }

            var firstCharge = leadDefendant.Charges
                .OrderBy(charge => charge.ListOrder)
                .FirstOrDefault();


            if (firstCharge != null && firstCharge.Code != NotYetChargedCode)
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

        private bool FindIsCaseCharged(IEnumerable<DefendantAndChargesDto> defendants)
        {
            return defendants
                .SelectMany(defendant => defendant.Charges)
                .Where(charge => charge.Code != NotYetChargedCode)
                .Any();
        }

        private IEnumerable<PcdRequestDto> MapPreChargeDecisionRequests(IEnumerable<DdeiPcdRequestDto> preChargeDecisionRequests)
        {
            return preChargeDecisionRequests.Select(pcdr => MapPreChargeDecisionRequest(pcdr));
        }

        private PcdCaseOutlineLineDto MapPcdCaseOutlineLine(DdeiPcdCaseOutlineLineDto ol)
        {
            return new PcdCaseOutlineLineDto
            {
                Heading = ol.Heading,
                Text = ol.Text,
                TextWithCmsMarkup = ol.TextWithCmsMarkup
            };
        }
        private PcdCommentsDto MapPreChargeDecisionComments(DdeiPcdCommentsDto comments)
        {
            return new PcdCommentsDto
            {
                Text = comments.Text,
                TextWithCmsMarkup = comments.TextWithCmsMarkup,
            };
        }

        private PcdRequestSuspectDto MapPcdSuspect(DdeiPcdRequestSuspectDto requestSuspectDto)
        {
            return new PcdRequestSuspectDto
            {
                Surname = requestSuspectDto.Surname,
                FirstNames = requestSuspectDto.FirstNames,
                Dob = requestSuspectDto.Dob,
                BailConditions = requestSuspectDto.BailConditions,
                BailDate = requestSuspectDto.BailDate,
                RemandStatus = requestSuspectDto.RemandStatus,
                ProposedCharges = requestSuspectDto.ProposedCharges.Select(pc => MapPcdProposedCharge(pc)).ToList()
            };
        }

        private PcdProposedChargeDto MapPcdProposedCharge(DdeiPcdProposedChargeDto ddeiPcdProposedChargeDto)
        {
            return new PcdProposedChargeDto
            {
                Charge = ddeiPcdProposedChargeDto.Charge,
                EarlyDate = ddeiPcdProposedChargeDto.EarlyDate,
                LateDate = ddeiPcdProposedChargeDto.LateDate,
                Location = ddeiPcdProposedChargeDto.Location,
                Category = ddeiPcdProposedChargeDto.Category
            };
        }

        private bool AreStringsEqual(string a, string b) =>
        (
            string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            || string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase
        );
    }
}