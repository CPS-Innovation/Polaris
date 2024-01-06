using System;
using System.IO;
using Aspose.Cells;
using Aspose.Diagram;
using Aspose.Email;
using Aspose.Slides;
using Aspose.Words;
using Microsoft.Extensions.Logging;
using pdf_generator.Factories.Contracts;
using LoadFormat = Aspose.Words.LoadFormat;

namespace pdf_generator.Factories
{
	public class AsposeItemFactory : IAsposeItemFactory
	{
		private readonly ILogger<AsposeItemFactory> _logger;

		public AsposeItemFactory(ILogger<AsposeItemFactory> logger)
		{
			_logger = logger;
		}

		public Workbook CreateWorkbook(Stream inputStream, Guid correlationId) => new Workbook(inputStream);

		public Diagram CreateDiagram(Stream inputStream, Guid correlationId) => new Diagram(inputStream);

		public MailMessage CreateMailMessage(Stream inputStream, Guid correlationId) => MailMessage.Load(inputStream);

		public Document CreateMhtmlDocument(Stream inputStream, Guid correlationId) =>
			new Document(inputStream, new Aspose.Words.Loading.LoadOptions
			{
				LoadFormat = LoadFormat.Mhtml
			});

		public Aspose.Pdf.Document CreateHtmlDocument(Stream inputStream, Guid correlationId)
		{
			// TODO - https://dev.azure.com/CPSDTS/Information%20Management/_workitems/edit/21851
			// Aspose is splitting the HTML into sections with whitespace between them. 
			// Only a single PDF page is rendered with these gaps present
			// Looks like the definition of a "page" is not quite right, likely configured here
			var options = new Aspose.Pdf.HtmlLoadOptions();
			options.IsRenderToSinglePage = false;
			options.PageInfo.IsLandscape = false;
			options.PageLayoutOption = Aspose.Pdf.HtmlPageLayoutOption.None;

			return new Aspose.Pdf.Document(inputStream, options);
		}

		public Aspose.Imaging.Image CreateImage(Stream inputStream, Guid correlationId) => Aspose.Imaging.Image.Load(inputStream);

		public Presentation CreatePresentation(Stream inputStream, Guid correlationId) => new Presentation(inputStream);

		public Document CreateWordsDocument(Stream inputStream, Guid correlationId) => new Document(inputStream);

		public Aspose.Pdf.Document CreateRenderedPdfDocument(Stream inputStream, Guid correlationId) => new Aspose.Pdf.Document(inputStream);
	}
}