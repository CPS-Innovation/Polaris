using System;

namespace Common.Dto.Response
{
    public class ExtractTextResult
    {
        public int LineCount { get; set; }
        public DateTime OcrCompletedTime { get; set; }
        public int PageCount { get; set; }
        public int WordCount { get; set; }
        public bool IsSuccess { get; set; }
    }
}