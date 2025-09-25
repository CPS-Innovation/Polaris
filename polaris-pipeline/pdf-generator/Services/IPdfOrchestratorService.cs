using pdf_generator.Domain.Document;
using System.Threading.Tasks;
using pdf_generator.Models;

namespace pdf_generator.Services;

public interface IPdfOrchestratorService
{
    Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto);
}