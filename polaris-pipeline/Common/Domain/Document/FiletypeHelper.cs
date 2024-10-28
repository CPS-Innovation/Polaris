using System;
using System.IO;

namespace Common.Domain.Document
{
    public static class FileTypeHelper
    {
        public const FileType PseudoDocumentFileType = FileType.HTML;
        public static bool TryGetSupportedFileType(string fileName, out FileType fileType)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileType = 0;
                return false;
            }

            var fileExtension = Path.GetExtension(fileName)
                .Replace(".", string.Empty)
                .ToUpperInvariant();

            var isSupported = Enum.TryParse<FileType>(fileExtension, out var f);
            fileType = f;

            return isSupported;
        }
    }
}