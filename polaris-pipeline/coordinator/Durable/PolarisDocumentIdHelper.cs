using System;
using Common.Domain.Document;

namespace coordinator.Durable
{
    public static class PolarisDocumentIdHelper
    {
        public static string GetPolarisDocumentId(PolarisDocumentType polarisDocumentType, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // at the moment parentIds pass through here, which would normally be empty.
                return null;
            }

            return polarisDocumentType switch
            {
                PolarisDocumentType.CmsDocument => $"CMS-{value}",
                PolarisDocumentType.PcdRequest => $"PCD-{value}",
                PolarisDocumentType.DefendantsAndCharges => $"DAC-{value}",
                _ => throw new ArgumentOutOfRangeException(nameof(polarisDocumentType), polarisDocumentType, null)
            };
        }
    }
}