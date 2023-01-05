using System;
using Azure.Storage.Sas;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RumpoleGateway.Factories;
using RumpoleGateway.Wrappers;
using Xunit;

namespace RumpoleGateway.Tests.Factories
{
    public class BlobSasBuilderWrapperFactoryTests
    {
        [Fact]
        public void Create_ReturnsBlobSasBuilderWrapper()
        {
            var loggerMock = new Mock<ILogger<BlobSasBuilderWrapperFactory>>();
            var factory = new BlobSasBuilderWrapperFactory(loggerMock.Object);

            var blobSasBuilder = factory.Create(new BlobSasBuilder(), Guid.NewGuid());

            blobSasBuilder.Should().BeOfType<BlobSasBuilderWrapper>();
        }
    }
}
