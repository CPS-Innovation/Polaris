using System.IO;
using System.Reflection;

namespace TextExtractor.TestHarness.Extensions
{
    public static class FileExtensions
    {
        public static string GetFilePath(this string filename)
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            return $"{fileInfo.Directory.FullName}/SourceFiles/{filename}";
        }
    }
}