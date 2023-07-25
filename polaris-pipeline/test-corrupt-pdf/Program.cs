using Aspose.Pdf;

var sourceFilename = "C:\\dev\\CPS\\Issues\\Squashed Tables\\Error Scenario\\Input.PDF";
var destinationFilename = "C:\\dev\\CPS\\Issues\\Squashed Tables\\Error Scenario\\Processed.PDF";
var fileStream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read);
var memoryStream = new MemoryStream();
fileStream.CopyTo(memoryStream);
fileStream.Close();
memoryStream.Seek(0, SeekOrigin.Begin);

MemoryStream pdfStream = new MemoryStream(); 
var doc = new Aspose.Pdf.Document(memoryStream);
doc.Save(pdfStream, SaveFormat.Pdf);

pdfStream.Seek(0, SeekOrigin.Begin);

using (var outStream = new FileStream(destinationFilename, FileMode.Create, FileAccess.Write))
{
    pdfStream.CopyTo(outStream);
}

