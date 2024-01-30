using System;
using System.IO;
using System.Linq;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Devices;
using Microsoft.Extensions.Options;

namespace pdf_generator.Services.DocumentRedaction.Aspose.RedactionImplementations
{
    public class ImageConversionImplementation : IRedactionImplementation
    {
        private readonly ImageConversionOptions _imageConversionOptions;
        private readonly ImageDevice _imageDevice;

        public ImageConversionImplementation(IOptions<ImageConversionOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _imageConversionOptions = options.Value;
            _imageDevice = new JpegDevice(
                new Resolution(_imageConversionOptions.Resolution),
                _imageConversionOptions.QualityPercent);
        }

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

        public void FinaliseAnnotations(ref Document document)
        {
            for (var pageNumber = 1; pageNumber <= document.Pages.Count; pageNumber++)
            {
                var page = document.Pages[pageNumber];
                if (!page.Annotations.Any(annotation => annotation.AnnotationType == AnnotationType.Square))
                {
                    continue;
                }

                var pageToSwapOutRect = page.Rect.ToRect();
                var pageHeight = page.PageInfo.Height;
                var pageWidth = page.PageInfo.Width;

                // do not dispose `memoryStream` here, cannot be disposed until the document is saved
                var memoryStream = new MemoryStream();
                _imageDevice.Process(page, memoryStream);

                document.Pages.Delete(pageNumber);

                var pageToSwapIn = document.Pages.Insert(pageNumber);
                pageToSwapIn.SetPageSize(pageWidth, pageHeight);
                pageToSwapIn.Rect = Rectangle.FromRect(pageToSwapOutRect);

                pageToSwapIn.PageInfo.Margin.Bottom =
                    pageToSwapIn.PageInfo.Margin.Top =
                    pageToSwapIn.PageInfo.Margin.Right =
                    pageToSwapIn.PageInfo.Margin.Left = 0;

                var i = new Image()
                {
                    ImageStream = memoryStream,
                    IsApplyResolution = true
                };

                pageToSwapIn.Paragraphs.Add(i);
            }
        }

        public (ProviderType, string) GetProviderType()
        {
            return (ProviderType.ImageConversion, null);
        }
    }
}