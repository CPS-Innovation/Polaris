using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using PolarisGateway.Validators;
using Xunit;
using FluentValidation.TestHelper;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;

namespace PolarisGateway.Tests.Validators
{
    public class DocumentRedactionSaveRequestValidatorTests
    {
        private readonly Fixture _fixture;

        public DocumentRedactionSaveRequestValidatorTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Redactions_WhenEmpty_ReturnsValidationError()
        {
            var saveRequest = _fixture.Create<DocumentRedactionSaveRequestDto>();
            saveRequest.Redactions = null;

            var redactionValidator = new DocumentRedactionSaveRequestValidator();
            var validationResult = await redactionValidator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Redactions);
        }

        [Fact]
        public async Task Redactions_WhenZeroLength_ReturnsValidationError()
        {
            var saveRequest = _fixture.Create<DocumentRedactionSaveRequestDto>();
            saveRequest.Redactions = new List<RedactionDefinitionDto>();

            var redactionValidator = new DocumentRedactionSaveRequestValidator();
            var validationResult = await redactionValidator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Redactions);
        }

        [Fact]
        public void Redactions_CorrectChildValidator_Loaded()
        {
            var saveRequest = _fixture.Create<DocumentRedactionSaveRequestDto>();
            saveRequest.Redactions = new List<RedactionDefinitionDto>();

            var redactionValidator = new DocumentRedactionSaveRequestValidator();
            redactionValidator.ShouldHaveChildValidator(x => x.Redactions, typeof(RedactionValidator));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task Redaction_PageIndex_DoesNotReturnValidationError(int pageIndex)
        {
            var saveRequest = _fixture.Create<DocumentRedactionSaveRequestDto>();
            saveRequest.Redactions = _fixture.CreateMany<RedactionDefinitionDto>().ToList();
            saveRequest.Redactions[0].PageIndex = pageIndex;

            var redactionValidator = new DocumentRedactionSaveRequestValidator();
            var validationResult = await redactionValidator.TestValidateAsync(saveRequest);

            validationResult.ShouldNotHaveValidationErrorFor(x => x.Redactions[0].PageIndex);
        }
    }
}
