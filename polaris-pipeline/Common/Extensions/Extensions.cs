using System;
using Common.Domain.Document;
using Common.Domain.Exceptions;

namespace Common.Domain.Extensions
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
