using System.Collections.Generic;
using Common.Domain.Document;

namespace pdf_generator.Models;

public static class FileTypesForPdfServicesDto
{
    public static readonly List<FileType> WordsFileTypes =
    [
        //FileType.DOC,
        FileType.DOCX,
        FileType.DOCM,
        FileType.DOT,
        FileType.DOTM,
        FileType.DOTX,
        FileType.RTF,
        FileType.TXT,
        FileType.HTML,
        FileType.HTM,
        FileType.MHT,
        FileType.MHTML
    ];

    public static readonly List<FileType> HteFileTypes =
    [
        FileType.HTE
    ];
    public static readonly List<FileType> CellsFileTypes =
    [
        FileType.CSV,
        FileType.XLS,
        FileType.XLSX,
        FileType.XLSM,
        FileType.XLT,
    ];
    public static readonly List<FileType> SlidesFileTypes =
    [
        FileType.PPT,
        FileType.PPTX
    ];
    public static readonly List<FileType> ImagingFileTypes =
    [
        FileType.BMP,
        FileType.EMZ,
        FileType.GIF,
        FileType.JPG,
        FileType.JPEG,
        FileType.TIF,
        FileType.TIFF,
        FileType.PNG,
    ];
    public static readonly List<FileType> DiagramFileTypes =
    [
        FileType.VSD
    ];
    public static readonly List<FileType> EmailFileTypes =
    [
        FileType.EML,
        FileType.MSG
    ];
    public static readonly List<FileType> PdfRendererFileTypes =
    [
        FileType.PDF
    ];
    public static readonly List<FileType> XpsPdfRendererFileTypes =
    [
        FileType.PDF
    ];

    public static readonly List<FileType> DocumentRetrievalFileTypes =
    [
        FileType.DOC
    ];
}