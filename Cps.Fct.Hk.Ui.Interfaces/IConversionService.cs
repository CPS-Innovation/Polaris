// <copyright file="IConversionService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Interface for document conversion service that provides methods to convert documents to pdf for preview.
/// </summary>
public interface IConversionService
{
    /// <summary>
    /// Saves a downloaded document to temporary storage, either on a local file system
    /// or in a cloud blob container, depending on the implementation.
    /// </summary>
    /// <param name="downloadedDocument">The file stream result representing the downloaded document.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is true if the document is successfully saved; otherwise, false.
    /// </returns>
    Task<bool> SaveDownloadedDocumentToTemporaryStorageAsync(FileStreamResult downloadedDocument);

    /// <summary>
    /// Converts the given document (e.g., a raster image or text file) to a PDF and saves the result
    /// either to local storage or cloud blob storage, depending on the implementation.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the document to be converted to PDF.</param>
    /// <param name="firstPageOnly">A flag to indicate if only first page needs to be converted or all pages when set to false.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true);

    /// <summary>
    /// Converts a raster image (e.g., a .jpg, .png file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the raster image to be converted.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertRasterImageToPdfDocumentAsync(string tmpFileDownloadName);

    /// <summary>
    /// Extracts specific pages from a PDF document and saves them as a new PDF file.
    /// The output PDF is saved either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the PDF document to be processed.</param>
    /// <param name="firstPageOnly">A flag to indicate if only first page needs to be converted or all pages when set to false.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertPdfToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true);

    /// <summary>
    /// Converts a text file (e.g., .txt, .csv file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the text file to be converted.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertTxtToPdfDocumentAsync(string tmpFileDownloadName);

    /// <summary>
    /// Converts a html file (e.g., .hte, .htm, .html file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the text file to be converted.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertHtmlToPdfDocumentAsync(string tmpFileDownloadName);

    /// <summary>
    /// Converts a doc file (e.g., .doc, .docx, .docm file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the text file to be converted.</param>
    /// <param name="firstPageOnly">A flag to indicate if only first page needs to be converted or all pages when set to false.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertDocToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true);

    /// <summary>
    /// Converts an xml file (e.g., .xml file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the text file to be converted.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertXmlToPdfDocumentAsync(string tmpFileDownloadName);

    /// <summary>
    /// Converts an xls file (e.g., .xls, .xlsx file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the text file to be converted.</param>
    /// <param name="firstSheetOnly">A flag to indicate if only first sheet needs to be converted or all sheets when set to false.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertXlsToPdfDocumentAsync(string tmpFileDownloadName, bool firstSheetOnly = true);

    /// <summary>
    /// Converts an msg file (e.g., .msg file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the text file to be converted.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertMsgToPdfDocumentAsync(string tmpFileDownloadName);

    /// <summary>
    /// Converts an ppt file (e.g., .ppt and pptx file) to a PDF document and saves the result
    /// either to local storage or a cloud blob container.
    /// </summary>
    /// <param name="tmpFileDownloadName">The temporary file name of the text file to be converted.</param>
    /// <param name="firstSlideOnly">A flag to indicate if only first slide needs to be converted or all slides when set to false..</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result is a string containing
    /// the URL of the saved PDF document, or null if the conversion or save operation fails.
    /// </returns>
    Task<string?> ConvertPptToPdfDocumentAsync(string tmpFileDownloadName, bool firstSlideOnly = true);
}
