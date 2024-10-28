
using System.IO;

namespace Common.Dto.Response
{
    public class FileResult
    {
        public Stream Stream { get; set; }

        public string FileName { get; set; }
    }
}