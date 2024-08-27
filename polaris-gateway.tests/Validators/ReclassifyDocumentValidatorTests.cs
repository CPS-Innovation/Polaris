using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Request;
using FluentValidation.TestHelper;
using Xunit;

namespace PolarisGateway.Validators.Tests
{
    public class ReclassifyDocumentValidatorTests
    {
        private readonly Fixture _fixture;
        private readonly ReclassifyDocumentValidator _validator;

        public ReclassifyDocumentValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new ReclassifyDocumentValidator();
        }

        [Fact]
        public async Task ReclassifyDocument_WhenDocumentIdIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new DocumentReclassificationRequestDto
            {
                DocumentId = 0
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.DocumentId);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenDocumentTypeIdIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new DocumentReclassificationRequestDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = 0
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.DocumentTypeId);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenExhibitType_AndItemIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new DocumentReclassificationRequestDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Exhibit,
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>()
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Exhibit.Item);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenExhibitType_AndReferenceIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new DocumentReclassificationRequestDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Exhibit,
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>(),
                    Item = _fixture.Create<string>()
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Exhibit.Reference);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndWitnessIdIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new DocumentReclassificationRequestDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Statement,
                Statement = new ReclassificationStatement()
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Statement.WitnessId);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndStatementNoIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new DocumentReclassificationRequestDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Statement,
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>()
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Statement.StatementNo);
        }


        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndStatementDateIsInvalid_ReturnsValidationError()
        {
            var saveRequest = new DocumentReclassificationRequestDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Statement,
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = "32-01-2024"
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Statement.Date);
        }
    }
}