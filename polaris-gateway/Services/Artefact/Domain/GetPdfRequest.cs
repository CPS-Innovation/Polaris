using System;

namespace PolarisGateway.Services.Artefact.Domain;

/// <summary>
/// Represents a request to retrieve a PDF artefact.
/// </summary>
/// <param name="Urn">The case URN identifier.</param>
/// <param name="CaseId">The case identifier.</param>
/// <param name="MaterialId">The material identifier.</param>
/// <param name="DocumentId">The document identifier (version) of the material.</param>
/// <param name="IsOcrProcessed">Indicates whether OCR processing should be applied.</param>
/// <param name="ForceRefresh">Indicates whether to force a refresh bypassing cache.</param>
public record GetPdfRequest(
    string Urn,
    int CaseId,
    string MaterialId,
    long DocumentId,
    bool IsOcrProcessed = false,
    bool ForceRefresh = false);
