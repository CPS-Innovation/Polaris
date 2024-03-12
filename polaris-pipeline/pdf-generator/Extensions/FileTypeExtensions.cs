using System;
using Common.Domain.Document;

namespace pdf_generator.Domain.Extensions
{
    public static class FileTypeExtensions
    {
        public static FileType ToFileType(this string fileType)
        {
            var fileTypeToExamine = fileType.Contains('.')
                ? fileType.Replace(".", "")
                : fileType;

            if (int.TryParse(fileTypeToExamine, out _) || !Enum.TryParse(typeof(FileType), fileTypeToExamine, true, out var type))
                throw new FileTypeException(fileTypeToExamine);

            return (FileType)type;
        }
    }
}
