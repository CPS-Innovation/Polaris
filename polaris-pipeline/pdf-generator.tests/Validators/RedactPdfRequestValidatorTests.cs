using System.Linq;
using Xunit;
using FluentValidation.TestHelper;
using AutoFixture;
using FluentAssertions.Execution;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;

namespace pdf_generator.tests.Validators
{
    public class RedactPdfRequestValidatorTests
    {
        private readonly IFixture _fixture;
        private static RedactPdfRequestValidator RedactPdfRequestValidator => new();

        public RedactPdfRequestValidatorTests()
        {
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(12345, true)]
        public void Validate_CaseId(long caseId, bool isValid)
        {
            var testRequest = _fixture.Build<RedactPdfRequestDto>()
                .With(x => x.CaseId, caseId)
                .Create();

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(x => x.CaseId);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.CaseId);
            }
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("12345", true)]
        public void Validate_DocumentId(string documentId, bool isValid)
        {
            var testRequest = _fixture.Build<RedactPdfRequestDto>()
                .With(x => x.DocumentId, documentId)
                .Create();

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(x => x.DocumentId);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.DocumentId);
            }
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("12345", true)]
        public void Validate_FileName(string fileName, bool isValid)
        {
            var testRequest = _fixture.Build<RedactPdfRequestDto>()
                .With(x => x.FileName, fileName)
                .Create();

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(x => x.FileName);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.FileName);
            }
        }

        [Fact]
        public void Validate_RedactionDefinitions_EmptyList()
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = null;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            result.ShouldHaveValidationErrorFor(x => x.RedactionDefinitions);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(15, true)]
        public void Validate_RedactionDefinitions_PageIndex(int pageIndex, bool isValid)
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(2).ToList();
            testRequest.RedactionDefinitions[0].PageIndex = pageIndex;
            testRequest.RedactionDefinitions[1].PageIndex = pageIndex;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                using (new AssertionScope())
                {
                    result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[0].PageIndex");
                    result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[1].PageIndex");
                }
            }
            else
            {
                using (new AssertionScope())
                {
                    result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].PageIndex");
                    result.ShouldHaveValidationErrorFor("RedactionDefinitions[1].PageIndex");
                }
            }
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, false)]
        [InlineData(15, true)]
        public void Validate_RedactionDefinitions_Width(int width, bool isValid)
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(2).ToList();
            testRequest.RedactionDefinitions[0].Width = width;
            testRequest.RedactionDefinitions[1].Width = width;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                using (new AssertionScope())
                {
                    result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[0].Width");
                    result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[1].Width");
                }
            }
            else
            {
                using (new AssertionScope())
                {
                    result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].Width");
                    result.ShouldHaveValidationErrorFor("RedactionDefinitions[1].Width");
                }
            }
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, false)]
        [InlineData(15, true)]
        public void Validate_RedactionDefinitions_Height(int height, bool isValid)
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(2).ToList();
            testRequest.RedactionDefinitions[0].Height = height;
            testRequest.RedactionDefinitions[1].Height = height;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                using (new AssertionScope())
                {
                    result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[0].Height");
                    result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[1].Height");
                }
            }
            else
            {
                using (new AssertionScope())
                {
                    result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].Height");
                    result.ShouldHaveValidationErrorFor("RedactionDefinitions[1].Height");
                }
            }
        }

        [Fact]
        public void Validate_RedactionDefinitions_RedactionCoordinates_EmptyList()
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(2).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates = null;
            testRequest.RedactionDefinitions[1].RedactionCoordinates = _fixture.CreateMany<RedactionCoordinatesDto>(2).ToList();

            var result = RedactPdfRequestValidator.TestValidate(testRequest);

            using (new AssertionScope())
            {
                result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates");
                result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[1].RedactionCoordinates[0]");
            }
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(15, true)]
        [InlineData(3.44, true)]
        public void Validate_RedactionDefinitions_RedactionCoordinates_X1(double x1, bool isValid)
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates = _fixture.CreateMany<RedactionCoordinatesDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates[0].X1 = x1;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].X1");
            }
            else
            {
                result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].X1");
            }
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(15, true)]
        [InlineData(3.44, true)]
        public void Validate_RedactionDefinitions_RedactionCoordinates_X2(double x2, bool isValid)
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates = _fixture.CreateMany<RedactionCoordinatesDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates[0].X2 = x2;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].X2");
            }
            else
            {
                result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].X2");
            }
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(15, true)]
        [InlineData(3.44, true)]
        public void Validate_RedactionDefinitions_RedactionCoordinates_Y1(double y1, bool isValid)
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates = _fixture.CreateMany<RedactionCoordinatesDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates[0].Y1 = y1;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].Y1");
            }
            else
            {
                result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].Y1");
            }
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(15, true)]
        [InlineData(3.44, true)]
        public void Validate_RedactionDefinitions_RedactionCoordinates_Y2(double y2, bool isValid)
        {
            var testRequest = _fixture.Create<RedactPdfRequestDto>();
            testRequest.RedactionDefinitions = _fixture.CreateMany<RedactionDefinitionDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates = _fixture.CreateMany<RedactionCoordinatesDto>(1).ToList();
            testRequest.RedactionDefinitions[0].RedactionCoordinates[0].Y2 = y2;

            var result = RedactPdfRequestValidator.TestValidate(testRequest);
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].Y2");
            }
            else
            {
                result.ShouldHaveValidationErrorFor("RedactionDefinitions[0].RedactionCoordinates[0].Y2");
            }
        }
    }
}
