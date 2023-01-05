using System.Linq;
using BusinessDomain = RumpoleGateway.Domain.CaseData;
using ApiDomain = RumpoleGateway.CaseDataImplementations.Tde.Domain;
using System;
using System.Collections.Generic;

namespace RumpoleGateway.CaseDataImplementations.Tde.Mappers
{
    public class CaseDetailsMapper : ICaseDetailsMapper
    {
        private const string NotYetChargedCode = "NYC";

        public BusinessDomain.CaseDetailsFull MapCaseDetails(ApiDomain.CaseDetails caseDetails)
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

        private IEnumerable<BusinessDomain.Defendant> MapDefendants(ApiDomain.CaseDetails caseDetails)
        {
            return caseDetails.Defendants.Select(defendant => MapDefendant(defendant, caseDetails.PreChargeDecisionRequests));
        }

        private BusinessDomain.Defendant MapDefendant(ApiDomain.CaseDefendant defendant, IEnumerable<ApiDomain.PreChargeDecisionRequest> pcdRequests)
        {
            return new BusinessDomain.Defendant
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                DefendantDetails = MapDefendantDetails(defendant),
                CustodyTimeLimit = MapCustodyTimeLimit(defendant.CustodyTimeLimit),
                Charges = MapCharges(defendant),
                ProposedCharges = MapPropsedCharges(defendant, pcdRequests)
            };
        }

        private BusinessDomain.DefendantDetails MapDefendantDetails(ApiDomain.CaseDefendant defendant)
        {
            return new BusinessDomain.DefendantDetails
            {
                Id = defendant.Id,
                ListOrder = defendant.ListOrder,
                FirstNames = defendant.FirstNames,
                Surname = defendant.Surname,
                // todo: no organisation name in TDE?
                OrganisationName = defendant.Surname,
                Dob = defendant.Dob,
                isYouth = defendant.Youth,
                Type = defendant.Type
            };
        }

        private BusinessDomain.CustodyTimeLimit MapCustodyTimeLimit(ApiDomain.CustodyTimeLimit custodyTimeLimit)
        {
            return new BusinessDomain.CustodyTimeLimit
            {
                ExpiryDate = custodyTimeLimit.ExpiryDate,
                ExpiryDays = custodyTimeLimit.ExpiryDays,
                ExpiryIndicator = custodyTimeLimit.ExpiryIndicator
            };
        }

        private IEnumerable<BusinessDomain.Charge> MapCharges(ApiDomain.CaseDefendant defendant)
        {
            var charges = new List<BusinessDomain.Charge>();
            var nextHearingDate = defendant.NextHearing.Date;

            return defendant.Offences
                .Select(offence => MapCharge(offence, nextHearingDate));
        }

        private IEnumerable<BusinessDomain.ProposedCharge> MapPropsedCharges(ApiDomain.CaseDefendant defendant, IEnumerable<ApiDomain.PreChargeDecisionRequest> pcdRequests)
        {
            return pcdRequests.Where(pcdr => pcdr.Dob == defendant.Dob
                                && pcdr.FirstNames == defendant.FirstNames
                                && pcdr.Surname == defendant.Surname)
                            .SelectMany(pcdr => pcdr.PropsedCharges)
                            .Select(proposedCharge => MapPropsedCharge(proposedCharge));
        }

        private BusinessDomain.Charge MapCharge(ApiDomain.Offence offence, string nextHearingDate)
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

        private BusinessDomain.ProposedCharge MapPropsedCharge(ApiDomain.ProposedCharge proposedCharge)
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

        private BusinessDomain.HeadlineCharge MapHeadlineCharge(BusinessDomain.ProposedCharge propsedCharge)
        {
            return new BusinessDomain.HeadlineCharge
            {
                Charge = propsedCharge.Charge,
                Date = propsedCharge.Date
            };
        }
        private BusinessDomain.Defendant FindLeadDefendant(IEnumerable<BusinessDomain.Defendant> defendants, ApiDomain.CaseSummary caseSummary)
        {

            // todo: this is not ideal, TDE only gives us the names of the lead defendant, so not 100%
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

            var firstPropsedCharge = leadDefendant.ProposedCharges.FirstOrDefault();

            if (firstPropsedCharge != null)
            {
                return MapHeadlineCharge(firstPropsedCharge);
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