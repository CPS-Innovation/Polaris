using AutoFixture;
using Ddei.Mappers;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Common.Dto.Response.Document;
using Ddei.Domain.Response.Document;

namespace Ddei.tests.Mappers;

public class MdsCaseDocumentMapperTests
{
    private readonly ICaseDocumentMapper<MdsDocumentResponse> _mapper;
    private readonly MdsDocumentResponse _documentResponse;

    public MdsCaseDocumentMapperTests()
    {
        var fixture = new Fixture();
        _mapper = new CaseDocumentMapper();
        _documentResponse = fixture.Create<MdsDocumentResponse>();
    }

    [Fact]
    public void When_AllElementsArePresentInResponse_ReturnsCorrectValues()
    {
        var result = _mapper.Map(_documentResponse);

        using (new AssertionScope())
        {
            result.DocumentId.Should().Be(_documentResponse.Id);
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
            result.PresentationTitle.Should().Be(_documentResponse.PresentationTitle);
            result.VersionId.Should().Be(_documentResponse.VersionId);
            result.CmsDocType.DocumentCategory.Should().Be(_documentResponse.CmsDocCategory);
            result.CmsDocType.DocumentType.Should().Be(_documentResponse.DocumentType);
            result.CmsDocType.DocumentTypeId.Should().Be(_documentResponse.DocumentTypeId);
        }
    }

    [Fact]
    public void When_OriginalFileNameIsNullInResponse_ReturnsCorrectValues()
    {
        _documentResponse.PresentationTitle = null;

        var result = _mapper.Map(_documentResponse);

        using (new AssertionScope())
        {
            result.DocumentId.Should().Be(_documentResponse.Id);
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
            result.PresentationTitle.Should().Be(_documentResponse.PresentationTitle);
            result.VersionId.Should().Be(_documentResponse.VersionId);
            result.CmsDocType.DocumentCategory.Should().Be(_documentResponse.CmsDocCategory);
            result.CmsDocType.DocumentType.Should().Be(_documentResponse.DocumentType);
            result.CmsDocType.DocumentTypeId.Should().Be(_documentResponse.DocumentTypeId);
        }
    }

    [Fact]
    public void When_DocumentTypeIsNullInResponse_ReturnsCorrectValues()
    {
        _documentResponse.DocumentType = null;

        var result = _mapper.Map(_documentResponse);

        using (new AssertionScope())
        {
            result.DocumentId.Should().Be(_documentResponse.Id);
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
            result.PresentationTitle.Should().Be(_documentResponse.PresentationTitle);
            result.VersionId.Should().Be(_documentResponse.VersionId);
            result.CmsDocType.DocumentCategory.Should().Be(_documentResponse.CmsDocCategory);
            result.CmsDocType.DocumentType.Should().BeNull();
            result.CmsDocType.DocumentTypeId.Should().Be(_documentResponse.DocumentTypeId);
        }
    }

    [Fact]
    public void When_DocumentTypeIdIsNullInResponse_ReturnsCorrectValues_AndUnknownAsDocumentType()
    {
        _documentResponse.DocumentTypeId = null;

        var result = _mapper.Map(_documentResponse);

        using (new AssertionScope())
        {
            result.DocumentId.Should().Be(_documentResponse.Id);
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
            result.PresentationTitle.Should().Be(_documentResponse.PresentationTitle);
            result.VersionId.Should().Be(_documentResponse.VersionId);
            result.CmsDocType.DocumentCategory.Should().Be(_documentResponse.CmsDocCategory);
            result.CmsDocType.DocumentType.Should().Be(_documentResponse.DocumentType);
            result.CmsDocType.DocumentTypeId.Should().Be(DocumentTypeDto.UnknownDocumentType);
        }
    }
}
