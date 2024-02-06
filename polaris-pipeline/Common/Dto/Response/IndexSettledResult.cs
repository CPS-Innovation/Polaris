using System.Collections.Generic;

namespace Common.Dto.Response
{
    public class IndexSettledResult
    {
        public long TargetCount { get; set; }
        public List<long> RecordCounts { get; set; }
        public bool IsSuccess { get; set; }
    }
}