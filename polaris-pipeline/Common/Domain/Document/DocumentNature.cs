using System;
using System.Text.RegularExpressions;

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

    public static long RemoveStringPrefix(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentNullException(nameof(documentId));
        }

        var match = Regex.Match(
                   documentId,
                   $@"(?:{DocumentPrefix}|{PreChargeDecisionRequestPrefix}|{DefendantsAndChargesPrefix})-(\d+)",
                   RegexOptions.None,
                   TimeSpan.FromSeconds(1));

        return match.Success
            ? long.Parse(match.Groups[1].Value)
            : throw new ArgumentException($"Invalid document id: {documentId}. Expected format with a three letter prefix e.g.: '{DocumentPrefix}-123456'");
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
