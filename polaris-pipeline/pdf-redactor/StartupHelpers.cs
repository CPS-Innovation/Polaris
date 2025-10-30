using Microsoft.Extensions.Logging;

namespace pdf_redactor;

internal static class StartupHelpers
{

    internal static void SetAsposeLicence(ILogger<Program> logger)
    {
        try
        {
            const string licenceFileName = "Aspose.Total.NET.lic";
            new Aspose.Pdf.License().SetLicense(licenceFileName);
        }
        catch (Exception ex)
        {
            //throw new AsposeLicenseException(exception.Message);
            logger.LogError($"Aspose license not found or invalid. Running in evaluation mode. Error: {ex.Message}");
        }
    }
}