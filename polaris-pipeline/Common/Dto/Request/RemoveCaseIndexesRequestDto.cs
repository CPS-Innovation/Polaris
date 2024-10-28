using Common.Validators;

namespace Common.Dto.Request
{
    public class RemoveCaseIndexesRequestDto
    {
        [RequiredLongGreaterThanZero]
        public int CaseId { get; set; }
    }
}