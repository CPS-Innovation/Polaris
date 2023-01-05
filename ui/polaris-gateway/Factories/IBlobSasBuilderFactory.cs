using System;
using Azure.Storage.Sas;

namespace RumpoleGateway.Factories
{
    public interface IBlobSasBuilderFactory
    {
        BlobSasBuilder Create(string blobName, Guid correlationId);
    }
}