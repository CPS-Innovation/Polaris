using System;
using Azure.Storage.Sas;
using RumpoleGateway.Wrappers;

namespace RumpoleGateway.Factories
{
    public interface IBlobSasBuilderWrapperFactory
    {
        IBlobSasBuilderWrapper Create(BlobSasBuilder blobSasBuilder, Guid correlationId);
    }
}
