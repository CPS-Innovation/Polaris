using System;

namespace Common.Domain.Document;

public static class DocumentNature
{
    public const string DocumentPrefix = "CMS";
    public const string PreChargeDecisionRequestPrefix = "PCD";
    public const string DefendantsAndChargesPrefix = "DAC";

    public enum Types
    {
        Document = 0,
        PreChargeDecisionRequest = 1,
        DefendantsAndCharges = 2,
    }

    public static string GetStringPrefix(Types documentNature)
    {
        return documentNature switch
        {
            Types.Document => DocumentPrefix,
            Types.PreChargeDecisionRequest => PreChargeDecisionRequestPrefix,
            Types.DefendantsAndCharges => DefendantsAndChargesPrefix,
            _ => throw new ArgumentOutOfRangeException(nameof(documentNature), documentNature, null),
        };
    }

    public static Types GetType(string prefix)
    {
        return prefix switch
        {
            DocumentPrefix => Types.Document,
            PreChargeDecisionRequestPrefix => Types.PreChargeDecisionRequest,
            DefendantsAndChargesPrefix => Types.DefendantsAndCharges,
            _ => throw new ArgumentOutOfRangeException(nameof(prefix), prefix, null),
        };
    }
}
