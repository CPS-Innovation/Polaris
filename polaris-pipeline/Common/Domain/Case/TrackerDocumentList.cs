using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Domain.Case
{
    public static class TrackerDocumentList
    {
        public static TrackerDocument GetDocument(this List<TrackerDocument> documents, Guid polarisDocumentId)
        {
            return documents.FirstOrDefault(doc => doc.PolarisDocumentId == polarisDocumentId);
        }
    }
}
