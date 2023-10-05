using Aspose.Pdf;
using Aspose.Pdf.Annotations;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal static class Aspose
    {
        public static void DoRedactions()
        {
            new License().SetLicense("Aspose.Total.NET.lic");

            foreach (var inputFile in Directory.GetFiles("./before"))
            {
                var doc = new Document(inputFile);

                for (var pageIndex = 1; pageIndex < doc.Pages.Count; pageIndex++)
                {
                    for (var i = 0; i < 50; i++)
                    {
                        AttachAnnotation(
                            doc,
                            pageIndex,
                            i,
                            x: i * 12,
                            y: i * 16,
                            w: 10,
                            h: 10
                        );
                    }
                }
                doc.Save("./after/aspose/" + Path.GetFileName(inputFile));
            }
        }

        static void AttachAnnotation(Document doc, int pageIndex, int i, int x, int y, int w, int h)
        {
            var page = doc.Pages[pageIndex];
            double x1 = x;
            double x2 = x + w;
            double y1 = page.PageInfo.Height - y;
            double y2 = page.PageInfo.Height - y - h;

            var annotation = new RedactionAnnotation(page, new Rectangle(x1, y2, x2, y1))
            {
                FillColor = Color.Black,
                OverlayText = i.ToString(),
                Color = Color.White,
                FontSize = 8,
            };

            page.Annotations.Add(annotation);
            annotation.Redact();
        }
    }
}
