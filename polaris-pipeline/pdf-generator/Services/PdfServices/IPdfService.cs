using System.Threading.Tasks;
using pdf_generator.Domain.Document;
using pdf_generator.Models;

namespace pdf_generator.Services.PdfServices;

public interface IPdfService
{
    Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto);
}