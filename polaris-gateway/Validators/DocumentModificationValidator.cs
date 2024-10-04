using Common.Dto.Request.DocumentManipulation;
using FluentValidation;

namespace PolarisGateway.Validators
{
    public class DocumentModificationValidator : AbstractValidator<DocumentModificationDto>
    {
        public DocumentModificationValidator()
        {
            RuleFor(x => x.PageIndex).NotEmpty().WithMessage("A page must be selected");
            RuleFor(x => x.Operation).NotEmpty();
            RuleFor(x => x.Arg).NotEmpty();
        }
    }
}