using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;
using Common.Extensions;

namespace Common.tests.Extensions;

public class HttpRequestExtensionsTests
{
  [Theory]
  [InlineData("", "")]
  [InlineData(null, "")]
  [InlineData("?", "?")]
  [InlineData("?foo=bar", "?foo=bar")]
  [InlineData("?.CMSAUTH%3DABC123%3B", "?.CMSAUTH%3DREDACTED%3B")]
  [InlineData("?foo.CMSAUTH%3DABC123%3B", "?foo.CMSAUTH%3DREDACTED%3B")]
  [InlineData("?foo.CMSAUTH%3DABC123", "?foo.CMSAUTH%3DREDACTED")]
  [InlineData("?foo.CMSAUTH%3DABC123%3Bbar", "?foo.CMSAUTH%3DREDACTED%3Bbar")]
  [InlineData("?foo.CMSAUTH%3DABC123%3Bbar%3Dbaz%3B", "?foo.CMSAUTH%3DREDACTED%3Bbar%3Dbaz%3B")]
  [InlineData("?foo.CMSAUTH=ABC123;bar", "?foo.CMSAUTH=REDACTED;bar")]
  public void GetLogSafeQueryString_ReturnsAStringWithCMSAuthInformationRedacted(string inputQueryString, string expectedQueryString)
  {
    var request = CreateMockHttpRequestWithQueryString(inputQueryString);
    request.GetLogSafeQueryString().Should().Be(expectedQueryString);
  }

  private HttpRequest CreateMockHttpRequestWithQueryString(string query)
  {
    var context = new DefaultHttpContext();
    var request = context.Request;
    request.ContentType = "application/json";
    request.QueryString = new QueryString(query);
    return request;
  }
}

