using System;
using System.IO;

namespace Common.Domain.Document
{
    public static class FiletypeHelper
    {
        public static FileType PseudoDocumentFileType = FileType.HTML;
        public static bool TryGetSupportedFileType(string fileName, out FileType fileType)
        {
            var fileExtension = Path.GetExtension(fileName)
                .Replace(".", string.Empty)
                .ToUpperInvariant();

            var isSupported = Enum.TryParse<FileType>(fileExtension, out var f);
            fileType = f;

            return isSupported;
        }
    }
}