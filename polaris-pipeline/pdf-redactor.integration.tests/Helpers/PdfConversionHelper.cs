using Aspose.Pdf;
using Aspose.Pdf.Devices;

public class PdfConversionHelper
{
  public static async Task<List<MemoryStream>> ConvertAndSavePdfToImages(Document pdfFile)
  {
    var tasks = new List<Task<MemoryStream>>();

    pdfFile.Pages.ToList().ForEach(page =>
    {
      Resolution resolution = new Resolution(300);
      PngDevice pngDevice = new PngDevice(resolution);

      var ms = new MemoryStream();
      tasks.Add(Task.Run(() =>
      {
        pngDevice.Process(page, ms);
        ms.Position = 0;
        return ms;
      }));
    });

    var streamResults = await Task.WhenAll(tasks);
    return streamResults.ToList();
  }

}