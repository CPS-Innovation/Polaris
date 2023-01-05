using System;
using Azure.Storage.Sas;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Factories
{
    public interface IBlobSasBuilderWrapperFactory
    {
        IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder, Guid correlationId);
    }
}
