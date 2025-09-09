//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Primitives;
//using Moq;
//using PolarisGateway.Functions;
//using PolarisGateway.Services.Artefact;
//using Xunit;

//namespace PolarisGateway.Tests.Functions;

//public class BulkRedactionSearchTests
//{
//    private readonly Mock<ILogger<PolarisPipelineBulkRedactionSearch>> _loggerMock;
//    private readonly Mock<IOcrArtefactService> _ocrArtefactServiceMock;
//    private const string SearchTextHeader = "SearchText";
//    private readonly PolarisPipelineBulkRedactionSearch _bulkRedactionSearch;

//    public BulkRedactionSearchTests()
//    {
//        _loggerMock = new Mock<ILogger<PolarisPipelineBulkRedactionSearch>>();
//        _ocrArtefactServiceMock = new Mock<IOcrArtefactService>();
//        _bulkRedactionSearch = new PolarisPipelineBulkRedactionSearch(_loggerMock.Object, _ocrArtefactServiceMock.Object);
//    }

//    [Fact]
//    public async Task Run_ShouldReturnOkObjectResult()
//    {
//        //arrange
//        var searchTerm = "search";
//        var req = new DefaultHttpContext().Request;
//        req.Query = new QueryCollection(new Dictionary<string, StringValues>(){{SearchTextHeader, searchTerm}});
//        var caseUrn = "Urn";
//        var caseId = 1;
//        var documentId = "2";
//        var versionId = 3;
//        var cancellationToken = CancellationToken.None;

//        //act
//        var result = await _bulkRedactionSearch.Run(req, caseUrn, caseId, documentId, versionId, cancellationToken);

//        //assert
//        Assert.IsType<OkObjectResult>(result);
//        _ocrArtefactServiceMock.Verify(v => v.GetOcrSearchRedactionsAsync(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId,documentId, versionId, searchTerm, cancellationToken ), Times.Once);
//    }
//}