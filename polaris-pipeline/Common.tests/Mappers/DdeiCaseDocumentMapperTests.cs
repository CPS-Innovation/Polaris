using AutoFixture;
using Common.Constants;
using Common.Dto.Response;
using Common.Mappers;
using Common.Mappers.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Common.Tests.Mappers;

public class DdeiCaseDocumentMapperTests
{
    private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _mapper;
    private readonly DdeiCaseDocumentResponse _documentResponse;

    public DdeiCaseDocumentMapperTests()
    {
        var fixture = new Fixture();
        _mapper = new DdeiCaseDocumentMapper();
        _documentResponse = fixture.Create<DdeiCaseDocumentResponse>();
    }

    [Fact]
    public void When_AllElementsArePresentInResponse_ReturnsCorrectValues()
    {
        var result = _mapper.Map(_documentResponse);

        using (new AssertionScope())
        {
            result.DocumentId.Should().Be(_documentResponse.Id.ToString());
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
            result.VersionId.Should().Be(_documentResponse.VersionId);
            result.CmsDocType.DocumentCategory.Should().Be(_documentResponse.CmsDocCategory);
            result.CmsDocType.DocumentType.Should().Be(_documentResponse.DocumentType);
            result.CmsDocType.DocumentTypeId.Should().Be(_documentResponse.DocumentTypeId);
        }
    }

    [Fact]
    public void When_OriginalFileNameIsNullInResponse_ReturnsCorrectValues()
    {
        _documentResponse.OriginalFileName = null;

        var result = _mapper.Map(_documentResponse);

        using (new AssertionScope())
        {
            result.DocumentId.Should().Be(_documentResponse.Id.ToString());
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
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
            result.DocumentId.Should().Be(_documentResponse.Id.ToString());
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
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
            result.DocumentId.Should().Be(_documentResponse.Id.ToString());
            result.FileName.Should().Be(_documentResponse.OriginalFileName);
            result.VersionId.Should().Be(_documentResponse.VersionId);
            result.CmsDocType.DocumentCategory.Should().Be(_documentResponse.CmsDocCategory);
            result.CmsDocType.DocumentType.Should().Be(_documentResponse.DocumentType);
            result.CmsDocType.DocumentTypeId.Should().Be(MiscCategories.UnknownDocumentType);
        }
    }
}
