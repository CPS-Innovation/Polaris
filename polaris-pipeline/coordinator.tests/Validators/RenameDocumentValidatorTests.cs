using Common.Dto.Request;
using coordinator.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace coordinator.tests.Validators
{
    public class RenameDocumentValidatorTests
    {
        private RenameDocumentValidator _validator;

        public RenameDocumentValidatorTests()
        {
            _validator = new RenameDocumentValidator();
        }

        [Fact]
        public void Should_Throw_Exception_When_DocumentId_Is_Empty()
        {
            var request = new RenameDocumentDto();
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.DocumentName);
        }

        [Fact]
        public void Should_Throw_Exception_When_DocumentName_Is_Empty()
        {
            var request = new RenameDocumentDto { };
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.DocumentName);
        }

        [Fact]
        public void Should_Throw_Exception_When_DocumentName_Length_Is_GreaterThan_100_Characters()
        {
            var request = new RenameDocumentDto
            {
                DocumentName = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras quis auctor nisl. Morbi porta, turpis id ornare vestibulum, quam est tincidunt velit, in tempus dui justo a felis. Sed quis bibendum magna, id dictum ligula. Proin porttitor mauris tortor, eu pretium erat commodo et. Proin vitae ipsum mi. Phasellus maximus ligula tellus, vel scelerisque diam rutrum ut. Vivamus quis maximus massa. Ut sollicitudin arcu non orci malesuada condimentum. Nullam vitae eros ut mi bibendum pellentesque posuere."
            };
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.DocumentName);
        }

        [Fact]
        public void Should_Not_Throw_Exception_When_Dto_IsValid()
        {
            var request = new RenameDocumentDto
            {
                DocumentName = "New document name"
            };
            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}