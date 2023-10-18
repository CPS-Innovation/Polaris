using System;
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Devices;

namespace pdf_generator.Services.DocumentRedactionService.RedactionProvider.ImageConversion
{
    public class ImageConversionProvider : IRedactionProvider
    {
        private readonly ImageDevice _imageDevice;

        public ImageConversionProvider(ImageConversionConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _imageDevice = new JpegDevice(new Resolution(config.Resolution), config.QualityPercent);
        }

        public ProviderType GetProviderType() => ProviderType.ImageConversion;

        public void AttachAnnotation(Page page, Rectangle rect)
        {
            var softAnnotation = new SquareAnnotation(page, rect)
            {
                InteriorColor = Color.Black,
                Color = Color.Black,
                ZIndex = int.MaxValue
            };
            page.Annotations.Add(softAnnotation, true);
        }

        public Document SanitizeDocument(Document document)
        {
            var outputDoc = new Document();
            foreach (var page in document.Pages)
            {
                var outputPage = outputDoc.Pages.Add();
                outputPage.PageInfo.Height = page.PageInfo.Height;
                outputPage.PageInfo.Width = page.PageInfo.Width;
                outputPage.PageInfo.Margin.Bottom =
                    outputPage.PageInfo.Margin.Top =
                    outputPage.PageInfo.Margin.Right =
                    outputPage.PageInfo.Margin.Left = 0;

                // do not dispose `memoryStream` here, cannot be disposed until the document is saved
                var memoryStream = new MemoryStream();
                _imageDevice.Process(page, memoryStream);

                var outputImage = new Image()
                {
                    ImageStream = memoryStream
                };

                outputPage.Paragraphs.Add(outputImage);
            }

            return outputDoc;
        }
    }
}