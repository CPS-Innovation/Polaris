// <copyright file="ImageConversionImplementation.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;

using System.Diagnostics;
using global::Aspose.Pdf;
using global::Aspose.Pdf.Annotations;
using global::Aspose.Pdf.Devices;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Common.Logging;
using pdf_redactor.Domain;
using Common.Wrappers;

public class ImageConversionImplementation(IOptions<ImageConversionOptions> imageConversionOptions, ILogger<ImageConversionImplementation> logger, IJsonConvertWrapper jsonConvertWrapper)
    : IRedactionImplementation
{
    private readonly ImageDevice imageDevice = new JpegDevice(
        new Resolution(imageConversionOptions.Value.Resolution),
        imageConversionOptions.Value.QualityPercent);

    public void AttachAnnotation(Page page, Rectangle rect)
    {
        var softAnnotation = new SquareAnnotation(page, rect)
        {
            InteriorColor = Color.Black,
            Color = Color.Black,
            ZIndex = int.MaxValue,
        };
        page.Annotations.Add(softAnnotation, true);
    }

    public void FinaliseAnnotations(ref Document document, Guid correlationId)
    {
        for (var pageNumber = 1; pageNumber <= document.Pages.Count; pageNumber++)
        {
            var stopwatch = Stopwatch.StartNew();

            var redactionAnnotationsEntity = new RedactionAnnotationsEntity
            {
                PageNumber = pageNumber,
            };

            var page = document.Pages[pageNumber];
            if (!page.Annotations.Any(annotation => annotation.AnnotationType == AnnotationType.Square))
            {
                continue;
            }

            // Note: Aspose page.Rect.ToRect() behaves differently to Aspose page.GetPageRect(true)
            // it switches the height and width for certain pdfs so we need to use GetPageRect(true) instead
            var pageRect = page.GetPageRect(true);

            // do not dispose `memoryStream` here, cannot be disposed until the document is saved
            var memoryStream = new MemoryStream();
            this.imageDevice.Process(page, memoryStream);
            redactionAnnotationsEntity.ImageConversionDurationMilliseconds = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            document.Pages.Delete(pageNumber);
            redactionAnnotationsEntity.DeletePageDurationMilliseconds = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            var pageToSwapIn = document.Pages.Insert(pageNumber);
            pageToSwapIn.SetPageSize(pageRect.Width, pageRect.Height);
            pageToSwapIn.Rect = pageRect;

            pageToSwapIn.PageInfo.Margin.Bottom =
                pageToSwapIn.PageInfo.Margin.Top =
                pageToSwapIn.PageInfo.Margin.Right =
                pageToSwapIn.PageInfo.Margin.Left = 0;

            if (pageRect.Width > pageRect.Height)
            {
                // if width of the original page is greater than the height, then the page is in landscape
                pageToSwapIn.PageInfo.IsLandscape = true;
            }

            var i = new Image()
            {
                ImageStream = memoryStream,
                IsApplyResolution = true,
                FixHeight = pageRect.Height,
                FixWidth = pageRect.Width,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            pageToSwapIn.Paragraphs.Add(i);

            stopwatch.Stop();
            redactionAnnotationsEntity.InsertPageDurationMilliseconds = stopwatch.ElapsedMilliseconds;

            redactionAnnotationsEntity.TotalFinaliseAnnotationsDurationMilliseconds =
                redactionAnnotationsEntity.ImageConversionDurationMilliseconds +
                redactionAnnotationsEntity.DeletePageDurationMilliseconds +
                redactionAnnotationsEntity.InsertPageDurationMilliseconds;

            logger.LogMethodFlow(correlationId, nameof(this.FinaliseAnnotations), jsonConvertWrapper.SerializeObject(redactionAnnotationsEntity));
        }
    }

    public (ProviderType, string?) GetProviderType()
    {
        return (ProviderType.ImageConversion, null);
    }
}
