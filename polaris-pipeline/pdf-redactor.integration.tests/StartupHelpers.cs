namespace pdf_redactor.integration.tests;

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
      throw new Exception(exception.Message);
    }
  }
}
