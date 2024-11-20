﻿using System;
using System.IO;
using Aspose.Diagram;
using FluentAssertions;
using Moq;
using pdf_generator.Factories.Contracts;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.PdfService
{
    public class DiagramPdfServiceTests
    {
        private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
        private readonly IPdfService _pdfService;

        public DiagramPdfServiceTests()
        {
            _asposeItemFactory = new Mock<IAsposeItemFactory>();
            _asposeItemFactory.Setup(x => x.CreateDiagram(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new Diagram());

            _pdfService = new DiagramPdfService(_asposeItemFactory.Object);
        }

        [Fact]
        public void Ctor_NoItemFactory_ThrowsAppropriateException()
        {
            var act = () =>
            {
                var _ = new DiagramPdfService(null);
            };

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("asposeItemFactory");
        }
        // todo: following test fails on mac (at least Stef's mac at time of writing)
#if Windows
        [Fact]
        public void ReadToPdfStream_CallsCreateDiagram()
        {
            using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestDiagram.vsd");

            var conversionResult = _pdfService.ReadToPdfStream(inputStream, "test-document-id", Guid.NewGuid());

            using (new AssertionScope())
            {
                _asposeItemFactory.Verify(x => x.CreateDiagram(It.IsAny<Stream>(), It.IsAny<Guid>()));
                conversionResult.Should().NotBeNull();
                conversionResult.ConvertedDocument.Should().NotBeNull();
                conversionResult.ConvertedDocument.Length.Should().BeGreaterThan(0);
            }
        }
#endif
    }
}
