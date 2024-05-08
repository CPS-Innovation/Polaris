using Common.Validators;

namespace Common.Dto.Request
{
    public class WaitForCaseEmptyResultsRequestDto
    {
        [RequiredLongGreaterThanZero]
        public long CaseId { get; set; }
    }
}