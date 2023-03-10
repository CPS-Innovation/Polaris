using Azure.Storage.Sas;
using System;

namespace Common.Factories.Contracts;

public interface IBlobSasBuilderFactory
{
    BlobSasBuilder Create(string blobName, Guid correlationId);
}
