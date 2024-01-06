using AutoFixture;
using Common.Constants;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Common.Factories;
using Common.Factories.Contracts;

namespace Common.Tests.Factories;

public class BlobSasBuilderFactoryTests
{
    private readonly string _blobName;
    private readonly string _blobContainerName;
    private readonly int _blobExpirySecs;
    private readonly Guid _correlationId;

    private readonly IBlobSasBuilderFactory _blobSasBuilderFactory;

    public BlobSasBuilderFactoryTests()
    {
        var fixture = new Fixture();
        _blobName = fixture.Create<string>();
        _blobContainerName = fixture.Create<string>();
        _blobExpirySecs = fixture.Create<int>();
        _correlationId = fixture.Create<Guid>();
        var configuration = new Mock<IConfiguration>();

        configuration.Setup(x => x[ConfigKeys.SharedKeys.BlobServiceContainerName]).Returns(_blobContainerName);
        configuration.Setup(x => x[ConfigKeys.SharedKeys.BlobExpirySecs]).Returns(_blobExpirySecs.ToString());

        _blobSasBuilderFactory = new BlobSasBuilderFactory(configuration.Object);
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithExpectedBlobServiceContainerName()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobName, _correlationId);

        sasBuilder.BlobContainerName.Should().Be(_blobContainerName);
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithExpectedBlobName()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobName, _correlationId);

        sasBuilder.BlobName.Should().Be(_blobName);
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithExpectedResource()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobName, _correlationId);

        sasBuilder.Resource.Should().Be("b");
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithStartTimeBeforeNow()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobName, _correlationId);

        sasBuilder.StartsOn.Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithExpectedExpiresOn()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobName, _correlationId);

        sasBuilder.ExpiresOn.Should().Be(sasBuilder.StartsOn.AddSeconds(_blobExpirySecs));
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithExpectedPermissions()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobName, _correlationId);

        sasBuilder.Permissions.Should().Be("r");
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithExpectedContentType()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobContainerName, _correlationId);

        sasBuilder.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public void Create_ReturnsSasBuilderWithExpectedContentDisposition()
    {
        var sasBuilder = _blobSasBuilderFactory.Create(_blobName, _correlationId);

        sasBuilder.ContentDisposition.Should().Be($"inline; filename={_blobName}");
    }
}

