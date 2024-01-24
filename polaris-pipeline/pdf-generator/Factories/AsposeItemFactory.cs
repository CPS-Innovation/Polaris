using System;
using System.IO;
using Aspose.Cells;
using Aspose.Diagram;
using Aspose.Email;
using Aspose.Pdf;
using Aspose.Slides;
using pdf_generator.Factories.Contracts;
using Document = Aspose.Words.Document;
using HtmlLoadOptions = Aspose.Pdf.HtmlLoadOptions;
using Image = Aspose.Imaging.Image;
using WordLoadFormat = Aspose.Words.LoadFormat;
using WordLoadOptions = Aspose.Words.Loading.LoadOptions;

namespace pdf_generator.Factories
{
	public class AsposeItemFactory : IAsposeItemFactory
	{
		public AsposeItemFactory()
		{
		}

		public Workbook CreateWorkbook(Stream inputStream, Guid correlationId) =>
			new Workbook(inputStream);

		public Diagram CreateDiagram(Stream inputStream, Guid correlationId) =>
			new Diagram(inputStream);

		public MailMessage CreateMailMessage(Stream inputStream, Guid correlationId) =>
			MailMessage.Load(inputStream);

		public Document CreateMhtmlDocument(Stream inputStream, Guid correlationId) =>
			new Document(inputStream, new WordLoadOptions { LoadFormat = WordLoadFormat.Mhtml });

		public Aspose.Pdf.Document CreateHtmlDocument(Stream inputStream, Guid correlationId)
		{
			// TODO - https://dev.azure.com/CPSDTS/Information%20Management/_workitems/edit/21851
			// Aspose is splitting the HTML into sections with whitespace between them. 
			// Only a single PDF page is rendered with these gaps present
			// Looks like the definition of a "page" is not quite right, likely configured here
			var options = new HtmlLoadOptions
			{
				IsRenderToSinglePage = false,
				PageInfo =
				{
					IsLandscape = false
				},
				PageLayoutOption = HtmlPageLayoutOption.None
			};

			return new Aspose.Pdf.Document(inputStream, options);
		}

		public Image CreateImage(Stream inputStream, Guid correlationId) =>
			Image.Load(inputStream);

		public Presentation CreatePresentation(Stream inputStream, Guid correlationId) =>
			new Presentation(inputStream);

		public Document CreateWordsDocument(Stream inputStream, Guid correlationId) =>
			new Document(inputStream);

		public Aspose.Pdf.Document CreateRenderedPdfDocument(Stream inputStream, Guid correlationId) =>
			new Aspose.Pdf.Document(inputStream);

		public Aspose.Pdf.Document CreateRenderedXpsPdfDocument(Stream inputStream, Guid correlationId) =>
			new Aspose.Pdf.Document(inputStream, new XpsLoadOptions());
	}
}