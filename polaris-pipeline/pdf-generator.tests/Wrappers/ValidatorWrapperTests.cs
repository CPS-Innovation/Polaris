using AutoFixture;
using Common.Domain.Requests;
using Common.Wrappers;
using FluentAssertions;
using Xunit;

namespace pdf_generator.tests.Wrappers
{
	public class ValidatorWrapperTests
	{
        private readonly Fixture _fixture;

        public ValidatorWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData("Test.doc")]
        [InlineData("Test2.xlsx")]
        [InlineData("UNUSED 1 - STORM LOG 1881 01.6.20 - EDITED 2020-11-23 MCLOVE.docx")]
        [InlineData("SDC items to be Disclosed (1-6) MCLOVE.docx")]
        [InlineData("!@£$%^&*().docx")]
        public void Validate_GeneratePdfRequest_ReturnsEmptyValidationResultsWhenFileNameIsValid(string fileName)
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.FileName, fileName)
                            .With(r => r.CaseId, 123456)
                            .With(r => r.VersionId, 123456)
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().BeEmpty();
        }

        [Fact]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenCaseIdIsMissing()
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                .With(r => r.CaseId, 0)
                .With(r => r.FileName, "TestFile.doc")
                .With(r => r.VersionId, 123456)
                .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenVersionIdIsMissing()
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                .With(r => r.CaseId, 123456)
                .With(r => r.DocumentId, "TestFile")
                .With(r => r.FileName, "TestFile.doc")
                .With(r => r.VersionId, 0)
                .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }

        [Fact]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenFileNameIsMissing()
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.FileName, default(string))
                            .With(r => r.CaseId, 123456)
                            .With(r => r.VersionId, 123456)
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("Test.so")]
        [InlineData("Test.somet")]
        public void Validate_GeneratePdfRequest_ReturnsNonEmptyValidationResultsWhenFileNameIsInvalid(string fileName)
        {
            var request = _fixture.Build<GeneratePdfRequest>()
                            .With(r => r.FileName, fileName)
                            .With(r => r.CaseId, 123456)
                            .With(r => r.VersionId, 123456)
                            .Create();

            var results = new ValidatorWrapper<GeneratePdfRequest>().Validate(request);

            results.Should().NotBeEmpty();
        }
    }
}

