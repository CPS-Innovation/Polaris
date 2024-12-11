using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;

namespace text_extractor.tests.Functions;

public abstract class BaseTestClass
{
    protected static Mock<HttpRequest> CreateMockRequest(object body, Guid? correlationId)
    {
        var mockRequest = new Mock<HttpRequest>();

        var ms = new MemoryStream();
        var sw = new StreamWriter(ms);

        var json = JsonConvert.SerializeObject(body);

        sw.Write(json);
        sw.Flush();

        ms.Position = 0;

        mockRequest.Setup(x => x.Body).Returns(ms);
        mockRequest.Setup(x => x.ContentLength).Returns(ms.Length);

        var mockHeaders = new HeaderDictionary();

        if (correlationId.HasValue && correlationId.Value != Guid.Empty)
        {
            mockHeaders.Append("Correlation-Id", correlationId.Value.ToString());
        }

        mockRequest.Setup(x => x.Headers).Returns(mockHeaders);
        return mockRequest;
    }
}