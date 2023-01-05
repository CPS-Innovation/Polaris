using System;
using Azure.Storage.Sas;

namespace PolarisGateway.Factories
{
    public interface IBlobSasBuilderFactory
    {
        BlobSasBuilder Create(string blobName, Guid correlationId);
    }
}