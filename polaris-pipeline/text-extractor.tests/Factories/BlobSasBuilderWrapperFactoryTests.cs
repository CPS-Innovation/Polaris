using Azure.Storage.Sas;
using FluentAssertions;
using Common.Factories;
using Common.Wrappers;
using Xunit;

namespace text_extractor.tests.Factories
{
    public class BlobSasBuilderWrapperFactoryTests
    {
        [Fact]
        public void Create_ReturnsBlobSasBuilderWrapper()
        {
            var factory = new BlobSasBuilderWrapperFactory();

            var blobSasBuilder = factory.Create(new BlobSasBuilder());

            blobSasBuilder.Should().BeOfType<BlobSasBuilderWrapper>();
        }
    }
}
