using AutoFixture;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Services.DocumentToggle;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using coordinator.Services;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace coordinator.tests.Durable.Activity;

public class GetCaseDocumentsTests
{
    private readonly CaseDto _case;
    private readonly CmsDocumentDto[] _caseDocuments;
    private readonly PresentationFlagsDto[] _presentationFlags;
    private readonly CasePayload _payload;
    private readonly GetCaseDocuments _getCaseDocuments;
    private readonly Mock<IStateStorageService> _mockStateStorageService;
    private readonly Mock<IMdsClient> _mdsClientMock;

    public GetCaseDocumentsTests()
    {
        var fixture = new Fixture();
        _payload = fixture.Create<CasePayload>();
        _case = fixture.Create<CaseDto>();
        _caseDocuments = [
            fixture.Create<CmsDocumentDto>(),
            fixture.Create<CmsDocumentDto>()
        ];

        _presentationFlags = [
            fixture.Create<PresentationFlagsDto>(),
            fixture.Create<PresentationFlagsDto>()
        ];

        _mockStateStorageService = new Mock<IStateStorageService>();
        _mdsClientMock = new Mock<IMdsClient>();
        _mdsClientMock
            .Setup(client => client.GetCaseAsync(It.IsAny<MdsCaseIdentifiersArgDto>()))
            .ReturnsAsync(_case);

        var mockMdsCaseIdentifiersArgDto = fixture.Create<MdsCaseIdentifiersArgDto>();

        var mockMdsArgFactory = new Mock<IMdsArgFactory>();
        mockMdsArgFactory
            .Setup(factory => factory.CreateCaseIdentifiersArg(_payload.CmsAuthValues, _payload.CorrelationId, _payload.Urn, _payload.CaseId))
            .Returns(mockMdsCaseIdentifiersArgDto);

        _mdsClientMock
            .Setup(client => client.ListDocumentsAsync(mockMdsCaseIdentifiersArgDto))
            .ReturnsAsync(_caseDocuments);

        var mockDocumentToggleService = new Mock<IDocumentToggleService>();
        mockDocumentToggleService
            .Setup(service => service.GetDocumentPresentationFlags(_caseDocuments[0]))
            .Returns(_presentationFlags[0]);
        mockDocumentToggleService
            .Setup(service => service.GetDocumentPresentationFlags(_caseDocuments[1]))
            .Returns(_presentationFlags[1]);

        var mockLogger = new Mock<ILogger<GetCaseDocuments>>();

        _getCaseDocuments = new GetCaseDocuments(
            _mdsClientMock.Object,
            mockMdsArgFactory.Object,
            mockDocumentToggleService.Object,
            _mockStateStorageService.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
    {
        _payload.CaseId = 0;

        await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_payload));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Run_WhenAccessTokenIsNullOrWhitespace_ThrowsArgumentException(string cmsAuthValues)
    {
        _payload.CmsAuthValues = cmsAuthValues;

        await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_payload));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Run_WhenCaseUrnIsNullOrWhitespace_ThrowsArgumentException(string caseUrn)
    {
        _payload.Urn = caseUrn;

        await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_payload));
    }

    [Fact]
    public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
    {
        _payload.CorrelationId = Guid.Empty;

        await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_payload));
    }

    [Fact]
    public async Task Run_ReturnsCaseDocuments()
    {
        var caseDocuments = await _getCaseDocuments.Run(_payload);

        caseDocuments.CmsDocuments.Should().BeEquivalentTo(_caseDocuments);
    }
}