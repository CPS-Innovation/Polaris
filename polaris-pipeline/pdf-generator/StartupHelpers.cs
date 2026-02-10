using Microsoft.Extensions.Logging;
using System;

namespace pdf_generator;

internal static class StartupHelpers
{

    internal static void SetAsposeLicence(ILogger<Program> logger)
    {
        try
        {
            const string licenceFileName = "Aspose.Total.NET.lic";

            new Aspose.Cells.License().SetLicense(licenceFileName);
            new Aspose.Email.License().SetLicense(licenceFileName);
            new Aspose.Imaging.License().SetLicense(licenceFileName);
            new Aspose.Pdf.License().SetLicense(licenceFileName);
            new Aspose.Slides.License().SetLicense(licenceFileName);
            new Aspose.Words.License().SetLicense(licenceFileName);
        }
        catch (Exception ex)
        {
            logger.LogError($"Aspose license not found or invalid. Running in evaluation mode. Error: {ex.Message}");
        }
    }
}