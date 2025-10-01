// <copyright file="FileUtils.cs" company="Crown Prosecution Service">
// Copyright (c) Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Utils;

using MimeTypes;

public static class FileUtils
{
    private const string DefaultContentType = "application/octet-stream";

    public static string? GetMimeType(string filePath)
    {
        if (filePath == null)
        {
            return null;
        }

        if (!MimeTypeMap.TryGetMimeType(filePath, out string contentType))
        {
            contentType = DefaultContentType;
        }

        return contentType;
    }

    public static string? GetExtensionFromMimeType(string mimeType)
    {
        if (mimeType == null)
        {
            return null;
        }

        try
        {
            return MimeTypeMap.GetExtension(mimeType);
        }
        catch (ArgumentException)
        {
            return null; // Return null if the mime type is unknown
        }
    }
}
