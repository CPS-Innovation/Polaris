using Common.Domain.Document;
using pdf_generator.Services.PdfServices;

namespace pdf_generator.Factories;

public interface IPdfServiceFactory
{
    IPdfService Create(FileType fileType);
}