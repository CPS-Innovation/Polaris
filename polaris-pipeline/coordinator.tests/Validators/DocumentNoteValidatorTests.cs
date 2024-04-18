using Common.Dto.Request;
using coordinator.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace coordinator.tests.Validators
{
    public class DocumentNoteValidatorTests
    {
        private DocumentNoteValidator _validator;

        public DocumentNoteValidatorTests()
        {
            _validator = new DocumentNoteValidator();
        }

        [Fact]
        public void Should_Throw_Exception_When_Text_Is_Empty()
        {
            var request = new AddDocumentNoteDto();
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Text);
        }

        [Fact]
        public void Should_Throw_Exception_When_Text_Length_Is_GreaterThan_500_Characters()
        {
            var request = new AddDocumentNoteDto
            {
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras quis auctor nisl. Morbi porta, turpis id ornare vestibulum, quam est tincidunt velit, in tempus dui justo a felis. Sed quis bibendum magna, id dictum ligula. Proin porttitor mauris tortor, eu pretium erat commodo et. Proin vitae ipsum mi. Phasellus maximus ligula tellus, vel scelerisque diam rutrum ut. Vivamus quis maximus massa. Ut sollicitudin arcu non orci malesuada condimentum. Nullam vitae eros ut mi bibendum pellentesque posuere."
            };
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Text);
        }
    }
}