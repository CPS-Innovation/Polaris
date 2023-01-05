﻿using System;
using System.IO;
using System.Text;
using Aspose.Words;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using pdf_generator.Factories;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.PdfService
{
    public class WordsPdfServiceTests
    {
        private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
        private readonly IPdfService _pdfService;

        public WordsPdfServiceTests()
        {
            _asposeItemFactory = new Mock<IAsposeItemFactory>();
            _asposeItemFactory.Setup(x => x.CreateWordsDocument(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new Document());

            _pdfService = new WordsPdfService(_asposeItemFactory.Object);
        }

        [Fact]
        public void Ctor_NoItemFactory_ThrowsAppropriateException()
        {
            var act = () =>
            {
                var _ = new SlidesPdfService(null);
            };

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("asposeItemFactory");
        }

        [Fact]
        public void ReadToPdfStream_CallsCreateWordsDocument()
        {
            using var pdfStream = new MemoryStream();
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));

            _pdfService.ReadToPdfStream(inputStream, pdfStream, Guid.NewGuid());

            using (new AssertionScope())
            {
                _asposeItemFactory.Verify(x => x.CreateWordsDocument(It.IsAny<Stream>(), It.IsAny<Guid>()));
                pdfStream.Should().NotBeNull();
                pdfStream.Length.Should().BeGreaterThan(0);
            }
        }
    }
}
