
using System.IO;
using Common.Constants;

namespace Common.Clients.PdfGeneratorDomain.Domain;

public class ConvertToPdfResponse
{
  public Stream PdfStream { get; set; }
  public PdfConversionStatus Status { get; set; }
}