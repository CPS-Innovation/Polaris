using System;
using System.IO;
using System.Text;
using Aspose.Pdf;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using pdf_generator.Factories.Contracts;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.PdfService
{
    public class XpsPdfRendererServiceTests
    {
        private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
        private readonly IPdfService _pdfService;

        public XpsPdfRendererServiceTests()
        {
            _asposeItemFactory = new Mock<IAsposeItemFactory>();
            _asposeItemFactory.Setup(x => x.CreateRenderedXpsPdfDocument(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new Document());

            _pdfService = new XpsPdfRendererService(_asposeItemFactory.Object);
        }

        [Fact]
        public void Ctor_NoItemFactory_ThrowsAppropriateException()
        {
            var act = () =>
            {
                var _ = new XpsPdfRendererService(null);
            };

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("asposeItemFactory");
        }

        [Fact]
        public void ReadToPdfStream_CallsCreateRenderedXpsPdfDocument()
        {
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));

            var conversionResult = _pdfService.ReadToPdfStream(inputStream, "test-document-id", Guid.NewGuid());

            using (new AssertionScope())
            {
                _asposeItemFactory.Verify(x => x.CreateRenderedXpsPdfDocument(It.IsAny<Stream>(), It.IsAny<Guid>()));
                conversionResult.Should().NotBeNull();
                conversionResult.ConvertedDocument.Should().NotBeNull();
                conversionResult.ConvertedDocument.Length.Should().BeGreaterThan(0);
            }
        }
    }
}
