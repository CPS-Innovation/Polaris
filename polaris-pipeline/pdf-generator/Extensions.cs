using System;
using pdf_generator.Domain;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator
{
    public static class Extensions
    {
        public static FileType ToFileType(this string fileType)
        {
            var fileTypeToExamine = (fileType.Contains('.')) ? fileType.Replace(".", "") : fileType;
            
            if(int.TryParse(fileTypeToExamine, out _) || !Enum.TryParse(typeof(FileType), fileTypeToExamine, true, out var type))
                throw new UnsupportedFileTypeException(fileTypeToExamine);
            
            return type == null ? FileType.TXT : (FileType)type;
        }
    }
}
