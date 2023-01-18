using System;
using System.IO;
using Aspose.Cells;
using Aspose.Diagram;
using Aspose.Email;
using Aspose.Slides;
using Common.Logging;
using Microsoft.Extensions.Logging;
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

		public Aspose.Words.Document CreateMhtmlDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateMhtmlDocument), string.Empty);
			
			var result = new Aspose.Words.Document(inputStream, new Aspose.Words.Loading.LoadOptions { LoadFormat = LoadFormat.Mhtml });
			_logger.LogMethodExit(correlationId, nameof(CreateMhtmlDocument), string.Empty);
			return result;
		}

		public Aspose.Pdf.Document CreateHtmlDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateHtmlDocument), string.Empty);
			
			var result = new Aspose.Pdf.Document(inputStream, new Aspose.Pdf.HtmlLoadOptions());
			_logger.LogMethodExit(correlationId, nameof(CreateHtmlDocument), string.Empty);
			return result;
		}

		public Aspose.Imaging.Image CreateImage(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateImage), string.Empty);
			
			var result = Aspose.Imaging.Image.Load(inputStream);
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

		public Aspose.Words.Document CreateWordsDocument(Stream inputStream, Guid correlationId)
		{
			_logger.LogMethodEntry(correlationId, nameof(CreateWordsDocument), string.Empty);
			
			var result = new Aspose.Words.Document(inputStream);
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
	}
}

