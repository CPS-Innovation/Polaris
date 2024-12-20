using Common.Constants;
using System;

namespace PolarisGateway.Services.Artefact.Domain;

public class ArtefactResult<T>
{
    public ResultStatus Status { get; set; }

    // if Status == ArtefactAvailable
    public T Artefact { get; set; }

    // if Status == PollWithToken
    public Guid ContinuationToken { get; set; }

    // if Status == Failed
    public PdfConversionStatus? FailedStatus { get; set; }

    // Diagnostic property
    public bool? IsFromStorage { get; set; }
}