
namespace pdf_generator.Services.DocumentRedaction
{
  public interface IUploadFileNameFactory
  {
    string BuildUploadFileName(string fileName);
  }
}