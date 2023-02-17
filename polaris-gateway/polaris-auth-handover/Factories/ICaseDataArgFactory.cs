using System;
using PolarisAuthHandover.Domain.CaseData.Args;

namespace PolarisAuthHandover.Factories
{
    public interface ICaseDataArgFactory
    {
        UrnArg CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);

        CaseArg CreateCaseArg(string cmsAuthValues, Guid correlationId, string urn, int caseId);

        CaseArg CreateCaseArgFromUrnArg(UrnArg arg, int caseId);
    }
}

