using System;
using AutoFixture;
using Common.Constants;
using FluentAssertions;
using Moq;
using text_extractor.Factories;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace text_extractor.tests.Factories
{
    public class BlobSasBuilderFactoryTests
    {
        private readonly string _blobName;
        private readonly string _blobContainerName;
        private readonly int _blobExpirySecs;

        private readonly IBlobSasBuilderFactory _blobSasBuilderFactory;

        public BlobSasBuilderFactoryTests()
        {
            var fixture = new Fixture();
            _blobName = fixture.Create<string>();
            _blobContainerName = fixture.Create<string>();
            _blobExpirySecs = fixture.Create<int>();
            var configuration = new Mock<IConfiguration>();

            configuration.Setup(x => x[ConfigKeys.SharedKeys.BlobServiceContainerName]).Returns(_blobContainerName);
            configuration.Setup(x => x[ConfigKeys.SharedKeys.BlobExpirySecs]).Returns(_blobExpirySecs.ToString());
            
            _blobSasBuilderFactory = new BlobSasBuilderFactory(configuration.Object);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedBlobContainerName()
        {
            var sasBuilder = _blobSasBuilderFactory.Create(_blobName);

            sasBuilder.BlobContainerName.Should().Be(_blobContainerName);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedBlobName()
        {
            var sasBuilder = _blobSasBuilderFactory.Create(_blobName);

            sasBuilder.BlobName.Should().Be(_blobName);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedResource()
        {
            var sasBuilder = _blobSasBuilderFactory.Create(_blobName);

            sasBuilder.Resource.Should().Be("b");
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithStartTimeBeforeNow()
        {
            var sasBuilder = _blobSasBuilderFactory.Create(_blobName);

            sasBuilder.StartsOn.Should().BeBefore(DateTimeOffset.UtcNow);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedExpiresOn()
        {
            var sasBuilder = _blobSasBuilderFactory.Create(_blobName);

            sasBuilder.ExpiresOn.Should().Be(sasBuilder.StartsOn.AddSeconds(_blobExpirySecs));
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedPermissions()
        {
            var sasBuilder = _blobSasBuilderFactory.Create(_blobName);

            sasBuilder.Permissions.Should().Be("r");
        }
    }
}
