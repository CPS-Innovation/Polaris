using Microsoft.AspNetCore.Http;
using Xunit;
using System.Net.Http.Headers;
using AutoFixture;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Extensions;
using FluentAssertions;

namespace Common.tests.Extensions;

public class HttpRequestHeadersExtensionsTests
{
    private readonly Fixture _fixture;


    public HttpRequestHeadersExtensionsTests()
    {
        _fixture = new Fixture();
    }


    [Fact]
    public void GetCorrelationId_WhenHeadersAreNull_ThrowsBadRequestException()
    {
        // Arrange
        HeaderDictionary headers = null;

        // Act
        Action act = () => headers.GetCorrelationId();

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetCorrelationId_WhenCorrelationIdIsNotPresent_ThrowsBadRequestException()
    {
        // Arrange
        var headers = new HeaderDictionary();

        // Act
        Action act = () => headers.GetCorrelationId();

        // Assert
        Assert.Throws<BadRequestException>(act);
    }

    [Fact]
    public void GetCorrelationId_WhenCorrelationIdIsPresent_ReturnsCorrelationId()
    {
        var correlationId = _fixture.Create<Guid>();
        // Arrange
        var headers = new HeaderDictionary{
            {HttpHeaderKeys.CorrelationId, correlationId.ToString()}
        };

        // Act
        var result = headers.GetCorrelationId();

        // Assert
        result.Should().Be(correlationId);
    }

    [Fact]
    public void GetCorrelationIdHttpHeadersOverload_WhenHeadersAreNull_ThrowsBadRequestException()
    {
        // Arrange
        HttpRequestHeaders headers = null;

        // Act
        Action act = () => headers.GetCorrelationId();

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetCorrelationIdHttpHeadersOverload_WhenCorrelationIdIsNotPresent_ThrowsBadRequestException()
    {
        // Arrange
        var headers = new HttpClient().DefaultRequestHeaders;

        // Act
        Action act = () => headers.GetCorrelationId();

        // Assert
        Assert.Throws<BadRequestException>(act);
    }

    [Fact]
    public void GetCorrelationIdHttpHeadersOverload_WhenCorrelationIdIsPresent_ReturnsCorrelationId()
    {
        var correlationId = _fixture.Create<Guid>();
        // Arrange
        var headers = new HttpClient().DefaultRequestHeaders;
        headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());

        // Act
        var result = headers.GetCorrelationId();

        // Assert
        result.Should().Be(correlationId);
    }

    [Fact]
    public void GetCmsAuthValuesHttpHeadersOverload_WhenHeadersAreNull_ThrowsBadRequestException()
    {
        // Arrange
        HttpRequestHeaders headers = null;

        // Act
        Action act = () => headers.GetCmsAuthValues();

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetCmsAuthValuesHttpHeadersOverload_WhenGetCmsAuthValuesIsNotPresent_ThrowsBadRequestException()
    {
        // Arrange
        var headers = new HttpClient().DefaultRequestHeaders;

        // Act
        Action act = () => headers.GetCmsAuthValues();

        // Assert
        Assert.Throws<BadRequestException>(act);
    }

    [Fact]
    public void GetCmsAuthValuesHttpHeadersOverload_WhenGetCmsAuthValuesIsEmpty_ThrowsBadRequestException()
    {
        // Arrange
        var headers = new HttpClient().DefaultRequestHeaders;
        headers.Add(HttpHeaderKeys.CmsAuthValues, string.Empty);

        // Act
        Action act = () => headers.GetCmsAuthValues();

        // Assert
        Assert.Throws<BadRequestException>(act);
    }


    [Fact]
    public void GetCmsAuthValuesHttpHeadersOverload_WhenGetCmsAuthValuesPresent_ReturnsGetCmsAuthValues()
    {
        var cmsAuthValues = _fixture.Create<string>();
        // Arrange
        var headers = new HttpClient().DefaultRequestHeaders;
        headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);

        // Act
        var result = headers.GetCmsAuthValues();

        // Assert
        result.Should().Be(cmsAuthValues);
    }
}