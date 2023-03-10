using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Common.Constants;
using Common.Factories.Contracts;
using Common.Services.SasGeneratorService;
using Common.Wrappers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.Tests.Services.SasGeneratorService;

public class SasGeneratorServiceTests
{
    private readonly string _blobName;
    private readonly Guid _correlationId;
    private readonly BlobUriBuilder _blobUriBuilder;

    private readonly ISasGeneratorService _sasGeneratorService;

    public SasGeneratorServiceTests()
    {
        var fixture = new Fixture();
        _blobName = fixture.Create<string>();
        var blobContainerName = fixture.Create<string>();
        var blobUserDelegationKeyExpirySecs = fixture.Create<int>();
        _correlationId = fixture.Create<Guid>();
        var blobSasBuilder = fixture.Create<BlobSasBuilder>();

        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var mockBlobSasBuilderFactory = new Mock<IBlobSasBuilderFactory>();
        var mockBlobSasBuilderWrapperFactory = new Mock<IBlobSasBuilderWrapperFactory>();
        var mockResponse = new Mock<Response<UserDelegationKey>>();
        var mockUserDelegationKey = new Mock<UserDelegationKey>();
        var mockBlobSasBuilderWrapper = new Mock<IBlobSasBuilderWrapper>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.BlobServiceContainerName]).Returns(blobContainerName);
        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.BlobUserDelegationKeyExpirySecs]).Returns(blobUserDelegationKeyExpirySecs.ToString());
    
        mockResponse.Setup(response => response.Value).Returns(mockUserDelegationKey.Object);
        mockBlobServiceClient.Setup(client => client.GetUserDelegationKeyAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);
        mockBlobServiceClient.Setup(client => client.Uri).Returns(fixture.Create<Uri>());

        _blobUriBuilder = new BlobUriBuilder(new Uri($"{mockBlobServiceClient.Object.Uri}{blobContainerName}/{_blobName}"));
        mockBlobSasBuilderFactory.Setup(factory => factory.Create(_blobUriBuilder.BlobName, _correlationId)).Returns(blobSasBuilder);
        mockBlobSasBuilderWrapper.Setup(wrapper => wrapper.ToSasQueryParameters(mockUserDelegationKey.Object, mockBlobServiceClient.Object.AccountName, _correlationId))
            .Returns(new Mock<SasQueryParameters>().Object.As<BlobSasQueryParameters>());
        mockBlobSasBuilderWrapperFactory.Setup(factory => factory.Create(blobSasBuilder)).Returns(mockBlobSasBuilderWrapper.Object);

        var loggerMock = new Mock<ILogger<Common.Services.SasGeneratorService.SasGeneratorService>>();
        _sasGeneratorService = new Common.Services.SasGeneratorService.SasGeneratorService(mockBlobServiceClient.Object, mockBlobSasBuilderFactory.Object, mockBlobSasBuilderWrapperFactory.Object, mockConfiguration.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GenerateSasUrl_ReturnsExpectedUri()
    {
        var response = await _sasGeneratorService.GenerateSasUrlAsync(_blobName, _correlationId);

        response.Should().Be(_blobUriBuilder.ToUri().ToString());
    }
}

