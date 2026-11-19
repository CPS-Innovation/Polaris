using System;
using System.Text.RegularExpressions;
using Common.Exceptions;

namespace Common.Domain.Document;

/// <summary>
/// The parsed representation of a route materialId: its document nature, numeric id value,
/// and the internal qualified id (e.g. "CMS-123") required by downstream services.
/// </summary>
/// <param name="Nature">The parsed document nature.</param>
/// <param name="NumericId">The numeric id value.</param>
/// <param name="QualifiedId">The internal qualified id (e.g. "CMS-123", "PCD-456", "DAC-789").</param>
public readonly record struct MaterialIdentifier(DocumentNature.Types Nature, long NumericId, string QualifiedId)
{
    public bool IsRedactable => DocumentNature.IsRedactable(Nature);
}

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
            : throw new BadRequestException($"Invalid document id: {documentId}. Expected format with a three letter prefix e.g.: '{prefix}-123456'", documentId);
    }

    public static string ToQualifiedStringDocumentId(long documentId, Types type) => ToQualifiedStringDocumentId(documentId.ToString(), type);

    public static string ToQualifiedStringDocumentId(string documentId, Types type) => $"{GetStringPrefix(type)}-{documentId}";

    /// <summary>
    /// Returns true if the given document nature is redactable (CMS documents only).
    /// </summary>
    public static bool IsRedactable(Types type) => type == Types.Document;

    /// <summary>
    /// Parses a route materialId into its document nature and numeric id, converting it into the internal
    /// qualified format required by downstream services. Accepts bare numeric ids (treated as CMS), and
    /// PCD-/DAC- prefixed ids. CMS- prefixed ids are also accepted temporarily for backwards compatibility.
    /// Throws <see cref="BadRequestException"/> for anything else.
    /// </summary>
    public static MaterialIdentifier Parse(string materialId)
    {
        if (string.IsNullOrWhiteSpace(materialId))
        {
            throw new BadRequestException("Material id is required.", nameof(materialId));
        }

        if (Regex.IsMatch(materialId, @"^\d+$", RegexOptions.None, TimeSpan.FromSeconds(1)))
        {
            var numericId = long.Parse(materialId);
            return new MaterialIdentifier(Types.Document, numericId, ToQualifiedStringDocumentId(numericId, Types.Document));
        }

        var match = Regex.Match(
            materialId,
            $@"^({DocumentPrefix}|{PreChargeDecisionRequestPrefix}|{DefendantsAndChargesPrefix})-(\d+)$",
            RegexOptions.None,
            TimeSpan.FromSeconds(1));

        if (!match.Success)
        {
            throw new BadRequestException(
                $"Invalid material id: {materialId}. Expected a numeric id or a prefixed id in the format '{DocumentPrefix}-123', '{PreChargeDecisionRequestPrefix}-123' or '{DefendantsAndChargesPrefix}-123'.",
                nameof(materialId));
        }

        var type = match.Groups[1].Value switch
        {
            DocumentPrefix => Types.Document,
            PreChargeDecisionRequestPrefix => Types.PreChargeDecisionRequest,
            DefendantsAndChargesPrefix => Types.DefendantsAndCharges,
            _ => throw new ArgumentOutOfRangeException(nameof(materialId), materialId, null),
        };

        return new MaterialIdentifier(type, long.Parse(match.Groups[2].Value), materialId);
    }

    /// <summary>
    /// Normalizes an externally supplied material id into the internal qualified format required by
    /// downstream services. numeric ids are treated as CMS and PCD-/DAC- prefixed ids for
    /// type-aware endpoints. 
    /// </summary>
    public static string NormalizeMaterialId(string materialId) => Parse(materialId).QualifiedId;

    /// <summary>
    /// Parses a route materialId for CMS only endpoints, any other prefixes return 400.
    /// <see cref="BadRequestException"/> 
    public static MaterialIdentifier ParseCmsOnly(string materialId)
    {
        var identifier = Parse(materialId);

        if (identifier.Nature != Types.Document)
        {
            throw new BadRequestException(
                $"Invalid material id: {materialId}. This endpoint only supports CMS materials.",
                nameof(materialId));
        }

        return identifier;
    }

    public static Types GetDocumentNatureType(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentNullException(nameof(documentId));
        }

        if (!documentId.Contains('-'))
        {
            throw new ArgumentException($"Invalid document id: {documentId}. Expected format with a three letter prefix e.g.: 'CMS-123456'");
        }

        var prefix = documentId[..3];

        return prefix switch
        {
            DocumentPrefix => Types.Document,
            PreChargeDecisionRequestPrefix => Types.PreChargeDecisionRequest,
            DefendantsAndChargesPrefix => Types.DefendantsAndCharges,
            _ => throw new ArgumentOutOfRangeException(nameof(documentId), documentId, null),
        };
    }
}
