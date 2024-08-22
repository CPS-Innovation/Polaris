using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Request;
using coordinator.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace coordinator.tests.Validators
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
        public async Task Should_Throw_Exception_When_DocumentId_Is_Empty()
        {
            var request = new ReclassifyDocumentDto
            {
                ReclassificationType = ReclassificationType.Other
            };
            var result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(x => x.DocumentId);
        }

        [Fact]
        public async Task Should_Throw_Exception_When_DocumentTypeId_Is_Empty()
        {
            var request = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Other
            };
            var result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(x => x.DocumentTypeId);
        }

        [Fact]
        public async Task Should_Throw_Exception_When_ExhibitType_And_ExistingProducerOrWitnessId_Is_Empty()
        {
            var request = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Exhibit,
                Exhibit = new ReclassificationExhibit()
            };
            var result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(x => x.Exhibit.ExistingProducerOrWitnessId);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenExhibitType_AndItemIsEmpty_ReturnsValidationError()
        {
            var request = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Exhibit,
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>()
                }
            };

            var result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(x => x.Exhibit.Item);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenExhibitType_AndReferenceIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
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

            var result = await _validator.TestValidateAsync(saveRequest);

            result.ShouldHaveValidationErrorFor(x => x.Exhibit.Reference);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndWitnessIdIsEmpty_ReturnsValidationError()
        {
            var request = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Statement,
                Statement = new ReclassificationStatement()
            };

            var result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(x => x.Statement.WitnessId);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndStatementNoIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Statement,
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>()
                }
            };

            var result = await _validator.TestValidateAsync(saveRequest);

            result.ShouldHaveValidationErrorFor(x => x.Statement.StatementNo);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenIsRenamedIsTrue_AndDocumentNameIsEmpty_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                ReclassificationType = ReclassificationType.Other,
                IsRenamed = true
            };

            var result = await _validator.TestValidateAsync(saveRequest);

            result.ShouldHaveValidationErrorFor(x => x.DocumentName);
        }
    }
}