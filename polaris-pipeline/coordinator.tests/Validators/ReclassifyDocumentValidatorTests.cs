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
        private const string ValidDateString = "31-01-2024";
        private const string StringGreaterThan255 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut vehicula eros tristique metus sollicitudin accumsan. Donec elementum lectus eget enim scelerisque pellentesque. Mauris eros orci, laoreet sit amet interdum in, viverra ut risus. Nunc feugiat molestie.";

        public ReclassifyDocumentValidatorTests()
        {
            _fixture = new Fixture();
            _validator = new ReclassifyDocumentValidator();
        }

        [Fact]
        public async Task Should_Throw_Exception_When_DocumentTypeId_Is_Empty()
        {
            var request = new ReclassifyDocumentDto
            {

            };
            var result = await _validator.TestValidateAsync(request);

            result.ShouldHaveValidationErrorFor(x => x.DocumentTypeId);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenExhibitType_AndItemIsEmpty_ReturnsValidationError()
        {
            var request = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
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
        public async Task ReclassifyDocument_WhenExhbitType_AndStatementIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>(),
                    Item = _fixture.Create<string>(),
                    Reference = _fixture.Create<string>()
                },
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = ValidDateString
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Statement);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenExhbitType_AndOtherIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>(),
                    Item = _fixture.Create<string>(),
                    Reference = _fixture.Create<string>()
                },
                Other = new ReclassificationOther
                {
                    Used = true
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Other);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenExhbitType_AndImmediateIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>(),
                    Item = _fixture.Create<string>(),
                    Reference = _fixture.Create<string>()
                },
                Immediate = new ReclassificationImmediate()
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Immediate);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndWitnessIdIsEmpty_ReturnsValidationError()
        {
            var request = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
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
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>()
                }
            };

            var result = await _validator.TestValidateAsync(saveRequest);

            result.ShouldHaveValidationErrorFor(x => x.Statement.StatementNo);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndStatementDateIsInvalid_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = _fixture.Create<string>()
                }
            };

            var result = await _validator.TestValidateAsync(saveRequest);

            result.ShouldHaveValidationErrorFor(x => x.Statement.Date);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndExhibitIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = ValidDateString
                },
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>(),
                    Item = _fixture.Create<string>(),
                    Reference = _fixture.Create<string>()
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Exhibit);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndOtherIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = ValidDateString
                },
                Other = new ReclassificationOther
                {
                    Used = true
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Other);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenStatementType_AndImmediateIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = ValidDateString
                },
                Immediate = new ReclassificationImmediate()
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Immediate);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenOtherType_AndStatementIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = ValidDateString
                },
                Other = new ReclassificationOther
                {
                    Used = true
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Statement);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenOtherType_AndExhibitIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentTypeId = _fixture.Create<int>(),
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>(),
                    Item = _fixture.Create<string>(),
                    Reference = _fixture.Create<string>()
                },
                Other = new ReclassificationOther
                {
                    Used = true
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Exhibit);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenOtherType_AndImmediateIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Other = new ReclassificationOther
                {
                    Used = true
                },
                Immediate = new ReclassificationImmediate()
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Immediate);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenOtherType_AndDocumentNameIsNotNull_AndValueExceedsMaximumLength_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Other = new ReclassificationOther
                {
                    DocumentName = StringGreaterThan255,
                    Used = true
                },
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Other.DocumentName);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenImmediateType_AndDocumentNameIsNotNull_AndValueExceedsMaximumLength_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Immediate = new ReclassificationImmediate
                {
                    DocumentName = StringGreaterThan255
                },
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Immediate.DocumentName);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenImmediateType_AndExhibitIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Immediate = new ReclassificationImmediate(),
                Exhibit = new ReclassificationExhibit
                {
                    ExistingProducerOrWitnessId = _fixture.Create<int>(),
                    Item = _fixture.Create<string>(),
                    Reference = _fixture.Create<string>()
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Exhibit);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenImmediateType_AndStatementIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Immediate = new ReclassificationImmediate(),
                Statement = new ReclassificationStatement
                {
                    WitnessId = _fixture.Create<int>(),
                    StatementNo = _fixture.Create<int>(),
                    Date = ValidDateString
                }
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Statement);
        }

        [Fact]
        public async Task ReclassifyDocument_WhenImmediateType_AndOtherIsNotNull_ReturnsValidationError()
        {
            var saveRequest = new ReclassifyDocumentDto
            {
                DocumentId = _fixture.Create<int>(),
                DocumentTypeId = _fixture.Create<int>(),
                Immediate = new ReclassificationImmediate(),
                Other = new ReclassificationOther()
            };

            var validationResult = await _validator.TestValidateAsync(saveRequest);

            validationResult.ShouldHaveValidationErrorFor(x => x.Other);
        }
    }
}