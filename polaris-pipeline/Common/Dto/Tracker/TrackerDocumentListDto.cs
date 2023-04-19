using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Dto.Tracker
{
    public static class TrackerDocumentListDto
    {
        public static TrackerCmsDocumentDto GetDocument(this List<TrackerCmsDocumentDto> documents, Guid polarisDocumentId)
        {
            return documents.FirstOrDefault(doc => doc.PolarisDocumentId == polarisDocumentId);
        }
    }
}
