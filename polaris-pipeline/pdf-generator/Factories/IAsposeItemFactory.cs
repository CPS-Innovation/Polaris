using System;
using System.IO;
using Aspose.Cells;
using Aspose.Diagram;
using Aspose.Email;
using Aspose.Slides;

namespace pdf_generator.Factories
{
	public interface IAsposeItemFactory
	{
		public Workbook CreateWorkbook(Stream inputStream, Guid correlationId);

		public Diagram CreateDiagram(Stream inputStream, Guid correlationId);

		public MailMessage CreateMailMessage(Stream inputStream, Guid correlationId);

		public Aspose.Words.Document CreateMhtmlDocument(Stream inputStream, Guid correlationId);

		public Aspose.Pdf.Document CreateHtmlDocument(Stream inputStream, Guid correlationId);

		public Aspose.Imaging.Image CreateImage(Stream inputStream, Guid correlationId);

		public Presentation CreatePresentation(Stream inputStream, Guid correlationId);

		public Aspose.Words.Document CreateWordsDocument(Stream inputStream, Guid correlationId);

		public Aspose.Pdf.Document CreateRenderedPdfDocument(Stream inputStream, Guid correlationId);
	}
}

