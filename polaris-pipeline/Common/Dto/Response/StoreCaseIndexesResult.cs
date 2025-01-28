using System;

namespace Common.Dto.Response
{
    public class StoreCaseIndexesResult
    {
        public StoreCaseIndexesResult()
        {
        }

        public DateTime IndexStoredTime { get; set; }

        public bool IsSuccess { get; set; }

        public int LineCount { get; set; }
    }
}