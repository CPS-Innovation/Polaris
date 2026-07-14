using PolarisGateway.Services.Artefact.Domain;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PolarisGateway.Services.Artefact;

public interface IPdfArtefactService
{
    /// <summary>
    /// Retrieves a PDF artefact asynchronously.
    /// </summary>
    /// <param name="request">The request containing document identification and processing options.</param>
    /// <param name="cmsAuthValues">The CMS authentication values.</param>
    /// <param name="correlationId">The correlation identifier for tracking the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An artefact result containing the PDF stream.</returns>
    public Task<ArtefactResult<Stream>> GetPdfAsync(
        GetPdfRequest request,
        string cmsAuthValues,
        Guid correlationId,
        CancellationToken cancellationToken = default,
        HttpRequest req = null);
}