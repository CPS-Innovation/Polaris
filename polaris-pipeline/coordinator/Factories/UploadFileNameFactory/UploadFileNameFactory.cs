using System;

namespace coordinator.Factories.UploadFileNameFactory
{
  public class UploadFileNameFactory : IUploadFileNameFactory
  {
    public string BuildUploadFileName(string fileName)
    {
      var fileNameWithoutExtension = fileName.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) > -1
          ? fileName.Split(".pdf", StringSplitOptions.RemoveEmptyEntries)[0]
          : fileName;

      return $"{fileNameWithoutExtension}_{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.pdf";
    }
  }
}