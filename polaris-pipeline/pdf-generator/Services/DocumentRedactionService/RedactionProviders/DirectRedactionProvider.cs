using System;
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;

namespace pdf_generator.Services.DocumentRedactionService.RedactionProvider
{
    public class DirectRedactionProvider : IRedactionProvider
    {
        public ProviderType GetProviderType() => ProviderType.DirectRedaction;

        public void AttachAnnotation(Page page, Rectangle rect)
        {
            var hardAnnotation = new RedactionAnnotation(page, rect)
            {
                FillColor = Color.Black,
            };

            page.Annotations.Add(hardAnnotation, true);
            hardAnnotation.Redact();
        }

        public Document SanitizeDocument(Document document)
        {
            document.RemoveMetadata();

            if (IsCandidateForConversion(document))
            {
                document.Convert(
                    new MemoryStream(), // `Convert` streams feedback here, we are not interested currently
                    PdfFormat.v_1_7,
                    ConvertErrorAction.Delete);
            }

            return document;
        }

        private static bool IsCandidateForConversion(Document document)
        {
            return (document.PdfFormat is PdfFormat.v_1_0
                        or PdfFormat.v_1_1
                        or PdfFormat.v_1_2
                        or PdfFormat.v_1_3
                        or PdfFormat.v_1_4
                        or PdfFormat.v_1_5
                        or PdfFormat.v_1_6)
                    && document.Validate(new PdfFormatConversionOptions(PdfFormat.v_1_7));
        }
    }
}