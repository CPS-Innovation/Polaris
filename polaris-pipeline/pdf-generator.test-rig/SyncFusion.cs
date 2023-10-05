using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Redaction;

internal static class SyncFusion
{
    public static void DoRedactions()
    {
        var licenseString = File.ReadAllText("SyncFusion.lic");
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseString);

        foreach (var inputFile in Directory.GetFiles("./before"))
        {
            //Load the existing PDF document
            var docStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            var doc = new PdfLoadedDocument(docStream);
            //Get the first page from the document

            for (var pageIndex = 0; pageIndex < doc.Pages.Count; pageIndex++)
            {
                for (var i = 0; i < 50; i++)
                {
                    AttachAnnotation(
                        doc,
                        pageIndex,
                        i,
                        x: i * 12,
                        y: i * 16,
                        h: 10,
                        w: 10
                    );
                }
            }
            doc.Redact();

            //Creating the stream object
            using var stream = new MemoryStream();
            var opt = new PdfCompressionOptions()
            {
                CompressImages = true,
                RemoveMetadata = true,
            };
            doc.Compress(opt);
            doc.Save(stream);

            stream.Position = 0;

            using var streamToWriteTo = File.Open("./after/syncfusion/" + Path.GetFileName(inputFile), FileMode.Create);
            stream.CopyTo(streamToWriteTo);
            //Close the document
            doc.Close(true);
        }
    }

    static void AttachAnnotation(PdfLoadedDocument doc, int pageIndex, int i, int x, int y, int w, int h)
    {
        var page = doc.Pages[pageIndex] as PdfLoadedPage;

        //Create a redaction object
        var redaction = new PdfRedaction(new RectangleF(x, y, w, h))
        {
            FillColor = new PdfColor(Color.Black),
        };
        //Add a redaction object into the redaction collection of loaded page
        page.AddRedaction(redaction);

    }
}