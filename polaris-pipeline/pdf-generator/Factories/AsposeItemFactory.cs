using System;
using System.IO;
using Aspose.Cells;
using Aspose.Diagram;
using Aspose.Email;
using Aspose.Pdf;
using Aspose.Slides;
using Common.Logging;
using Microsoft.Extensions.Logging;
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
	    private readonly ILogger<AsposeItemFactory> _logger;

	    public AsposeItemFactory(ILogger<AsposeItemFactory> logger)
	    {
		    _logger = logger;
	    }
	    
        public Workbook CreateWorkbook(Stream inputStream, Guid correlationId)
        {
	        _logger.LogMethodEntry(correlationId, nameof(CreateWorkbook), string.Empty);
	        
			var result = new Workbook(inputStream);
			_logger.LogMethodExit(correlationId, nameof(CreateWorkbook), string.Empty);
			return result;
		}

		public Diagram CreateDiagram(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateDiagram), string.Empty);

			var result = new Diagram(inputStream);
			_logger.LogMethodExit(correlationId, nameof(CreateDiagram), string.Empty);
			return result;
		}

		public MailMessage CreateMailMessage(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateMailMessage), string.Empty);

			var result = MailMessage.Load(inputStream);
			_logger.LogMethodExit(correlationId, nameof(CreateMailMessage), string.Empty);
			return result;
		}

		public Document CreateMhtmlDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateMhtmlDocument), string.Empty);

			var result = new Document(inputStream, new WordLoadOptions { LoadFormat = WordLoadFormat.Mhtml });
			_logger.LogMethodExit(correlationId, nameof(CreateMhtmlDocument), string.Empty);
			return result;
		}

		public Aspose.Pdf.Document CreateHtmlDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateHtmlDocument), string.Empty);

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

            var document = new Aspose.Pdf.Document(inputStream, options);

			_logger.LogMethodExit(correlationId, nameof(CreateHtmlDocument), string.Empty);
			return document;
		}

		public Image CreateImage(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateImage), string.Empty);

			var result = Image.Load(inputStream);
			_logger.LogMethodExit(correlationId, nameof(CreateImage), string.Empty);
			return result;
		}

		public Presentation CreatePresentation(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreatePresentation), string.Empty);

			var result = new Presentation(inputStream);
			_logger.LogMethodExit(correlationId, nameof(CreatePresentation), string.Empty);
			return result;
		}

		public Document CreateWordsDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateWordsDocument), string.Empty);

			var result = new Document(inputStream);
			_logger.LogMethodExit(correlationId, nameof(CreateWordsDocument), string.Empty);
			return result;
		}

		public Aspose.Pdf.Document CreateRenderedPdfDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateRenderedPdfDocument), string.Empty);

			var result = new Aspose.Pdf.Document(inputStream);
			_logger.LogMethodExit(correlationId, nameof(CreateRenderedPdfDocument), string.Empty);
			return result;
		}

		public Aspose.Pdf.Document CreateRenderedXpsPdfDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateRenderedPdfDocument), string.Empty);

			var result = new Aspose.Pdf.Document(inputStream, new XpsLoadOptions());
			_logger.LogMethodExit(correlationId, nameof(CreateRenderedPdfDocument), string.Empty);
			return result;
		}
	}
}