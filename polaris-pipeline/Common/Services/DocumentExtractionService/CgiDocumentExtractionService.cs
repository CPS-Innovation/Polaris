using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Services.DocumentExtractionService.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Services.DocumentExtractionService;

public class CgiDocumentExtractionService : BaseDocumentExtractionService, ICgiDocumentExtractionService
{
    private readonly ILogger<CgiDocumentExtractionService> _logger;
    private readonly IConfiguration _configuration;
    private const long VersionIdPlaceholder = 123456; 
    
    public CgiDocumentExtractionService(HttpClient httpClient, IHttpRequestFactory httpRequestFactory, ILogger<CgiDocumentExtractionService> logger, IConfiguration configuration)
        : base(logger, httpRequestFactory, httpClient)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [ExcludeFromCodeCoverage]
    public Task<CmsCaseDocument[]> ListDocumentsAsync(string caseId, string accessToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(ListDocumentsAsync), caseId);

        var result = Task.FromResult(caseId switch
        {
            "18846" => McLoveCase(caseId),
            "18848" => MultipleFileTypeCase(caseId),
            _ => null
        }).Result;
        
        _logger.LogMethodExit(correlationId, nameof(ListDocumentsAsync), result.ToJson());
        return Task.FromResult(result.CaseDocuments.ToArray());
    }

    public async Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"DocumentId: {documentId}, FileName: {fileName}");
        //It has been assumed here that CDE will return 404 not found when document cant be found. Test this when hooked up properly
        var content = await GetHttpContentAsync(string.Format(_configuration[ConfigKeys.SharedKeys.GetDocumentUrl], documentId, fileName), accessToken, 
            string.Empty, correlationId);
        var result = await content.ReadAsStreamAsync();
        _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
        return result;
    }
    
    [ExcludeFromCodeCoverage]
    private static CmsCase McLoveCase(string caseId)
    {
        return new CmsCase
        {
            CaseId = caseId,
            CaseDocuments = new[]
            {
                new CmsCaseDocument
                {
                    DocumentId = "MG12",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG12.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG12",
                        DocumentCategory = "MG12 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "stmt Shelagh McLove MG11",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "stmt Shelagh McLove MG11.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG11",
                        DocumentCategory = "MG11 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG00",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG00.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG00",
                        DocumentCategory = "MG00 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "stmt JONES 1989 1 JUNE mg11",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "stmt JONES 1989 1 JUNE mg11.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG11",
                        DocumentCategory = "MG11 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG20 10 JUNE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG20 10 JUNE.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG20",
                        DocumentCategory = "MG20 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "UNUSED 1 - STORM LOG 1881 01.6.20 - EDITED 2020-11-23 MCLOVE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "UNUSED 1 - STORM LOG 1881 01.6.20 - EDITED 2020-11-23 MCLOVE.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG11",
                        DocumentCategory = "MG11 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "Shelagh McLove VPS mg11",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "Shelagh McLove VPS mg11.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG11",
                        DocumentCategory = "MG11 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "UNUSED 6 - DA CHECKLIST MCLOVE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "UNUSED 6 - DA CHECKLIST MCLOVE.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG6",
                        DocumentCategory = "MG6 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG0",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG0.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG0",
                        DocumentCategory = "MG0 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG06 3 June",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG06 3 June.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG06",
                        DocumentCategory = "MG06 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "SDC items to be Disclosed (1-6) MCLOVE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "SDC items to be Disclosed (1-6) MCLOVE.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG11",
                        DocumentCategory = "MG11 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "stmt BLAYNEE 2034 1 JUNE mg11",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "stmt BLAYNEE 2034 1 JUNE mg11.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG11",
                        DocumentCategory = "MG11 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "PRE CONS D",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "PRE CONS D.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG00",
                        DocumentCategory = "MG00 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG05 MCLOVE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG05 MCLOVE.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG05",
                        DocumentCategory = "MG05 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG20 5 JUNE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG20 5 JUNE.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG20",
                        DocumentCategory = "MG20 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG02 SHELAGH MCLOVE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG02 SHELAGH MCLOVE.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG02",
                        DocumentCategory = "MG02 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MG06 10 june",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MG06 10 june.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG06",
                        DocumentCategory = "MG06 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "stmt Lucy Doyle MG11",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "stmt Lucy Doyle MG11.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG11",
                        DocumentCategory = "MG11 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "MCLOVE MG3",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "MCLOVE MG3.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG3",
                        DocumentCategory = "MG3 File"
                    }
                }
            }
        };
    }

    [ExcludeFromCodeCoverage]
    private static CmsCase MultipleFileTypeCase(string caseId)
    {
        return new CmsCase
        {
            CaseId = caseId,
            CaseDocuments = new[]
            {
                new CmsCaseDocument
                {
                    DocumentId = "docCDE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "docCDE.doc",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG0",
                        DocumentCategory = "MG0 File"
                    }
                },
                new CmsCaseDocument
                {
                    DocumentId = "docxCDE",
                    VersionId = VersionIdPlaceholder,
                    FileName =  "docxCDE.docx",
                    CmsDocType = new CmsDocType
                    {
                        DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "docmCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "docmCDE.docm",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "xlsxCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "xlsxCDE.xlsx",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "xlsCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "xlsCDE.xls",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "pptCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "pptCDE.ppt",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "pptxCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "pptxCDE.pptx",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "htmlCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "htmlCDE.html",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "msgCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "msgCDE.msg",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "vsdCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "vsdCDE.vsd",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "bmpCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "bmpCDE.bmp",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "gifCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "gifCDE.gif",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "jpgCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "jpgCDE.jpg",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "pngCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "pngCDE.png",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "tiffCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "tiffCDE.tiff",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "rtfCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "rtfCDE.rtf",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            },
            new CmsCaseDocument
            {
                DocumentId = "txtCDE",
                VersionId = VersionIdPlaceholder,
                FileName =  "txtCDE.txt",
                CmsDocType = new CmsDocType
                {
                    DocumentType = "MG0",
                    DocumentCategory = "MG0 File"
                }
            }
        }
    };
    }
}