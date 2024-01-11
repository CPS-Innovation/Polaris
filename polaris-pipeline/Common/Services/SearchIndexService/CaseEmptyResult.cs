using System.Collections.Generic;

namespace Common.Services.CaseSearchService
{
    public class CaseEmptyResult
    {
        public List<long> RemainingIndexRecordCounts { get; set; }

        public bool IsSuccess { get; set; }
    }
}