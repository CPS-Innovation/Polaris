using pdf_thumbnail_generator.Domain.Exceptions;

namespace pdf_thumbnail_generator
{ 
    internal static class StartupHelpers 
    {
        internal static void SetAsposeLicence() 
        {
            try
            {
                const string licenceFileName = "Aspose.Total.NET.lic";
                new Aspose.Pdf.License().SetLicense(licenceFileName);
            }
            catch (Exception exception)
            {
                throw new AsposeLicenseException(exception.Message);
            }
        }
    }
}