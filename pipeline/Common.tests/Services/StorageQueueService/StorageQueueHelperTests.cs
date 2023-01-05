using AutoFixture;
using Common.Services.StorageQueueService;
using Common.Services.StorageQueueService.Contracts;
using FluentAssertions;
using Xunit;

namespace Common.tests.Services.StorageQueueService;

public class StorageQueueHelperTests
{
    private readonly Fixture _fixture;
    private readonly IStorageQueueHelper _sut;

    public StorageQueueHelperTests()
    {
        _fixture = new Fixture();

        _sut = new StorageQueueHelper();
    }

    [Fact]
    public void IsBase64Encoded_Check_ReturnsFalse_WhenANormalString_IsProvided()
    {
        var testMessage = _fixture.Create<string>();

        var testResult = _sut.IsBase64Encoded(testMessage);

        testResult.Should().BeFalse();
    }
    
    [Fact]
    public void IsBase64Encoded_Check_ReturnsTrue_WhenABase64EncodedString_IsProvided()
    {
        var testMessage = _fixture.Create<string>();
        var encodedTestMessage = _sut.Base64Encode(testMessage);

        var testResult = _sut.IsBase64Encoded(encodedTestMessage);

        testResult.Should().BeTrue();
    }
}
