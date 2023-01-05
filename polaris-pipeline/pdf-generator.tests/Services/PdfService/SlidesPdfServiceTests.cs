﻿using System;
using System.IO;
using Aspose.Slides;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using pdf_generator.Factories;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.PdfService
{
    public class SlidesPdfServiceTests
    {
        private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
        private readonly IPdfService _pdfService;

        public SlidesPdfServiceTests()
        {
            _asposeItemFactory = new Mock<IAsposeItemFactory>();
            _asposeItemFactory.Setup(x => x.CreatePresentation(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new Presentation());

            _pdfService = new SlidesPdfService(_asposeItemFactory.Object);
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
        public void ReadToPdfStream_CallsCreatePresentation()
        {
            using var pdfStream = new MemoryStream();
            using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestPresentation.pptx");

            _pdfService.ReadToPdfStream(inputStream, pdfStream, Guid.NewGuid());

            using (new AssertionScope())
            {
                _asposeItemFactory.Verify(x => x.CreatePresentation(It.IsAny<Stream>(), It.IsAny<Guid>()));
                pdfStream.Should().NotBeNull();
                pdfStream.Length.Should().BeGreaterThan(0);
            }
        }
    }
}
