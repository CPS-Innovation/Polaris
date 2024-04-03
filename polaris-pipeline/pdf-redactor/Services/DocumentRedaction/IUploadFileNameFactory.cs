
namespace pdf_redactor.Services.DocumentRedaction
{
  public interface IUploadFileNameFactory
  {
    string BuildUploadFileName(string fileName);
  }
}