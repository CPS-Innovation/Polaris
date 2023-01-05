using System;
using System.IO;
using System.Text;
using Aspose.Email;
using Aspose.Words;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using pdf_generator.Factories;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.PdfService
{
    public class EmailPdfServiceTests
    {
        private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
        private readonly IPdfService _pdfService;

        public EmailPdfServiceTests()
        {
            _asposeItemFactory = new Mock<IAsposeItemFactory>();
            _asposeItemFactory.Setup(x => x.CreateMailMessage(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new MailMessage());
            _asposeItemFactory.Setup(x => x.CreateMhtmlDocument(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new Document());

            _pdfService = new EmailPdfService(_asposeItemFactory.Object);
        }

        [Fact]
        public void Ctor_NoItemFactory_ThrowsAppropriateException()
        {
            var act = () =>
            {
                var _ = new EmailPdfService(null);
            };

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("asposeItemFactory");
        }

        [Fact]
        public void ReadToPdfStream_CallsCorrectMethodSequence()
        {
            using var pdfStream = new MemoryStream();
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));

            _pdfService.ReadToPdfStream(inputStream, pdfStream, Guid.NewGuid());

            using (new AssertionScope())
            {
                _asposeItemFactory.Verify(x => x.CreateMailMessage(It.IsAny<Stream>(), It.IsAny<Guid>()));
                _asposeItemFactory.Verify(x => x.CreateMhtmlDocument(It.IsAny<Stream>(), It.IsAny<Guid>()));
                pdfStream.Should().NotBeNull();
                pdfStream.Length.Should().BeGreaterThan(0);
            }
        }
    }
}
