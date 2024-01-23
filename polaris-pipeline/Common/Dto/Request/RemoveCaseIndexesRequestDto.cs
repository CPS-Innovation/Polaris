using Common.Validators;

namespace Common.Dto.Request
{
    public class RemoveCaseIndexesRequestDto
    {
        [RequiredLongGreaterThanZero]
        public long CaseId { get; set; }
    }
}