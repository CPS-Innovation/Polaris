using Common.Dto.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Services
{
    public interface IRedactionService
    {
        public Task<Stream> ProcessAsync(
                        int caseId,
                        string materialId,
                        long documentId,
                        RedactPdfRequestDto request,
                        CmsAuthValues cmsAuthValues,
                        Guid correlationId,
                        CancellationToken cancellationToken);
    }
}
