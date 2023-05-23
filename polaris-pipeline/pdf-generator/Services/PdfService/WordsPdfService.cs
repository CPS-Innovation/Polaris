using System.Xml.Linq;
using System;
using System.IO;
using Aspose.Words;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class WordsPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public WordsPdfService(IAsposeItemFactory asposeItemFactory)
        {
            try
            {
                var license = new License();
                license.SetLicense("Aspose.Total.NET.lic");
            }
            catch (Exception exception)
            {
                throw new AsposeLicenseException(exception.Message);
            }

            _asposeItemFactory = asposeItemFactory;
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            FileFormatInfo info = FileFormatUtil.DetectFileFormat(inputStream);

            var doc = new Document(inputStream, new Aspose.Words.Loading.LoadOptions
            {
                LoadFormat = LoadFormat.Doc,
                MswVersion = Aspose.Words.Settings.MsWordVersion.Word2000,
                WarningCallback = new WarningCallback()
            });

            doc.Save(pdfStream, SaveFormat.Pdf);

            pdfStream.Seek(0, SeekOrigin.Begin);
        }

        private class WarningCallback : IWarningCallback
        {
            public void Warning(WarningInfo info)
            {
                Console.WriteLine(info.WarningType);
                Console.WriteLine(info.Description);
            }
        }
    }
}
