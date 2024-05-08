using System;
using Common.Domain.Document;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace pdf_generator.Functions;

public static class ConvertToPdfHelper
{
    public const string FiletypeKey = "Filetype";

    public static FileType GetFileType(IHeaderDictionary headers)
    {
        if (headers == null)
            throw new ArgumentNullException(nameof(headers));

        if (!headers.TryGetValue(FiletypeKey, out var value))
            throw new BadRequestException("Missing Filetype Value", nameof(headers));

        var filetypeValue = value[0];
        if (string.IsNullOrEmpty(filetypeValue))
            throw new BadRequestException("Null Filetype Value", filetypeValue);
        if (!Enum.IsDefined(typeof(FileType), filetypeValue))
            throw new BadRequestException("Invalid Filetype Enum Value", filetypeValue);

        Enum.TryParse(filetypeValue, true, out FileType filetype);

        return filetype;
    }
}