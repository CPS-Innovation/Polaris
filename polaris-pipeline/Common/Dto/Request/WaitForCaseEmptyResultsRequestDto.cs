using Common.Validators;

namespace Common.Dto.Request
{
    public class WaitForCaseEmptyResultsRequestDto
    {
        [RequiredLongGreaterThanZero]
        public int CaseId { get; set; }
    }
}