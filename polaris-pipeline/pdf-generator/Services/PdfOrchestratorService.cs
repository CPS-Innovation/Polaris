using Common.Constants;
using Common.Logging;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories;
using pdf_generator.Models;
using System;
using System.Threading.Tasks;

namespace pdf_generator.Services;

public class PdfOrchestratorService : IPdfOrchestratorService
{
    private readonly ILogger<PdfOrchestratorService> _logger;
    private readonly IPdfServiceFactory _pdfServiceFactory;

    public PdfOrchestratorService(
        ILogger<PdfOrchestratorService> logger, 
        IPdfServiceFactory pdfServiceFactory)
    {
        _logger = logger;
        _pdfServiceFactory = pdfServiceFactory;
    }

    public async Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        _logger.LogMethodEntry(readToPdfDto.CorrelationId, nameof(ReadToPdfStreamAsync), readToPdfDto.DocumentId);
        PdfConversionResult conversionResult;
        var converterType = PdfConverterType.None;

        try
        {
            _logger.LogMethodFlow(readToPdfDto.CorrelationId, nameof(ReadToPdfStreamAsync), "Analysing file type and matching to a converter");

            var pdfService = _pdfServiceFactory.Create(readToPdfDto.FileType);
            conversionResult = await pdfService.ReadToPdfStreamAsync(readToPdfDto);
        }
        catch (Exception exception)
        {
            _logger.LogMethodError(readToPdfDto.CorrelationId, nameof(ReadToPdfStreamAsync), exception.Message, exception);
            conversionResult = new PdfConversionResult(readToPdfDto.DocumentId, converterType);
            conversionResult.RecordConversionFailure(PdfConversionStatus.UnexpectedError, exception.ToFormattedString());

            return conversionResult;
        }
        finally
        {
            _logger.LogMethodExit(readToPdfDto.CorrelationId, nameof(ReadToPdfStreamAsync), string.Empty);
        }

        return conversionResult;
    }
}