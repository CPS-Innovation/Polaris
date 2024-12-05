using System;
using System.Text.RegularExpressions;

namespace Common.Domain.Document;

public static class DocumentNature
{
    private const string DocumentPrefix = "CMS";
    private const string PreChargeDecisionRequestPrefix = "PCD";
    private const string DefendantsAndChargesPrefix = "DAC";

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

    public static long ToNumericDocumentId(string documentId, Types type)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentNullException(nameof(documentId));
        }

        var prefix = GetStringPrefix(type);
        var match = Regex.Match(
                   documentId,
                   $@"{prefix}-(\d+)",
                   RegexOptions.None,
                   TimeSpan.FromSeconds(1));

        return match.Success
            ? long.Parse(match.Groups[1].Value)
            : throw new ArgumentException($"Invalid document id: {documentId}. Expected format with a three letter prefix e.g.: '{prefix}-123456'");
    }

    public static string ToQualifiedStringDocumentId(long documentId, Types type) => ToQualifiedStringDocumentId(documentId.ToString(), type);

    public static string ToQualifiedStringDocumentId(string documentId, Types type) => $"{GetStringPrefix(type)}-{documentId}";

    public static Types GetType(string prefix) => prefix switch
    {
        DocumentPrefix => Types.Document,
        PreChargeDecisionRequestPrefix => Types.PreChargeDecisionRequest,
        DefendantsAndChargesPrefix => Types.DefendantsAndCharges,
        _ => throw new ArgumentOutOfRangeException(nameof(prefix), prefix, null),
    };
}
