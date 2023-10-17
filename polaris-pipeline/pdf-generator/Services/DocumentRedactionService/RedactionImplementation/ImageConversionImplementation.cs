using Aspose.Pdf;
using Aspose.Pdf.Annotations;

namespace pdf_generator.Services.DocumentRedactionService.RedactionImplementation
{
    public class ImageConversionImplementation : IRedactionImplementation
    {
        public ImplementationType GetImplementationType() => ImplementationType.ImageConversion;
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
            foreach (var inputPage in document.Pages)
            {
                var outputPage = outputDoc.Pages.Add();
                outputPage.PageInfo.Height = inputPage.PageInfo.Height;
                outputPage.PageInfo.Width = inputPage.PageInfo.Width;
                outputPage.PageInfo.Margin.Bottom =
                    outputPage.PageInfo.Margin.Top =
                    outputPage.PageInfo.Margin.Right =
                    outputPage.PageInfo.Margin.Left = 0;

                var outputImage = new Image()
                {
                    ImageStream = inputPage.ConvertToPNGMemoryStream()
                };

                outputPage.Paragraphs.Add(outputImage);
            }

            return outputDoc;
        }
    }
}