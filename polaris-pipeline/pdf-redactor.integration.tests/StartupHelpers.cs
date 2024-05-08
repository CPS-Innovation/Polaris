namespace pdf_redactor.integration.tests;

internal static class StartupHelpers
{

  internal static void SetAsposeLicence()
  {
    try
    {
      const string licenceFileName = "Aspose.Total.NET.lic";
      new Aspose.Cells.License().SetLicense(licenceFileName);
      new Aspose.Diagram.License().SetLicense(licenceFileName);
      new Aspose.Email.License().SetLicense(licenceFileName);
      new Aspose.Imaging.License().SetLicense(licenceFileName);
      new Aspose.Pdf.License().SetLicense(licenceFileName);
      new Aspose.Slides.License().SetLicense(licenceFileName);
      new Aspose.Words.License().SetLicense(licenceFileName);
    }
    catch (Exception exception)
    {
      throw new Exception(exception.Message);
    }
  }
}
