// <copyright file="DocumentTypeMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System.Collections.Generic;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Constants;
using Microsoft.Extensions.Logging;

/// <summary>
/// A service that maps document type IDs to document types.
/// </summary>
public class DocumentTypeMapper : IDocumentTypeMapper
{
    private readonly Dictionary<int, DocumentTypeInfo> documentTypeMapping;
    private readonly ILogger<DocumentTypeMapper> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeMapper"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used to log errors and warnings..</param>
    public DocumentTypeMapper(ILogger<DocumentTypeMapper> logger)
    {
        this.logger = logger;
        this.documentTypeMapping = new Dictionary<int, DocumentTypeInfo>
        {
            { 1201, new DocumentTypeInfo { DocumentType = "ABE", Category = "Other Material", Group = "Other" } },
            { -7, new DocumentTypeInfo { DocumentType = "Asset Recovery", Category = "Communication", Group = "Other" } },
            { -6, new DocumentTypeInfo { DocumentType = "Asset Recovery 3rd Party", Category = "Communication", Group = "Other" } },
            { -5, new DocumentTypeInfo { DocumentType = "Complaint", Category = "Communication", Group = "Other" } },
            { -4, new DocumentTypeInfo { DocumentType = "Counsel Case Acknowledgement ", Category = "Communication", Group = "Other" } },
            { 1064, new DocumentTypeInfo { DocumentType = "DCF", Category = "Communication", Group = "MG Form" } },
            { 1032, new DocumentTypeInfo { DocumentType = "Forwarded Internal Email", Category = "Communication", Group = "Other" } },
            { -1, new DocumentTypeInfo { DocumentType = "Correspondence", Category = "Communication", Group = "Other" } },
            { 1055, new DocumentTypeInfo { DocumentType = "DREP", Category = "MG Form", Group = "MG Form" } },
            { -2, new DocumentTypeInfo { DocumentType = "Defence Statement", Category = "Other Material", Group = "Other" } },
            { 1066, new DocumentTypeInfo { DocumentType = "MG00", Category = "MG Form", Group = "MG Form" } },
            { 1001, new DocumentTypeInfo { DocumentType = "MG1 (no longer received)", Category = "MG Form", Group = "MG Form" } },
            { 1033, new DocumentTypeInfo { DocumentType = "MG2", Category = "MG Form", Group = "MG Form" } },
            { 1034, new DocumentTypeInfo { DocumentType = "MG3", Category = "MG Form", Group = "MG Form" } },
            { 1035, new DocumentTypeInfo { DocumentType = "MG3A", Category = "MG Form", Group = "MG Form" } },
            { 1002, new DocumentTypeInfo { DocumentType = "MG4", Category = "MG Form", Group = "MG Form" } },
            { 1003, new DocumentTypeInfo { DocumentType = "MG4A", Category = "MG Form", Group = "MG Form" } },
            { 1004, new DocumentTypeInfo { DocumentType = "MG4B", Category = "MG Form", Group = "MG Form" } },
            { 1005, new DocumentTypeInfo { DocumentType = "MG4C", Category = "MG Form", Group = "MG Form" } },
            { 1060, new DocumentTypeInfo { DocumentType = "MG4D", Category = "MG Form", Group = "MG Form" } },
            { 1061, new DocumentTypeInfo { DocumentType = "MG4E", Category = "MG Form", Group = "MG Form" } },
            { 1037, new DocumentTypeInfo { DocumentType = "MG4(Post)", Category = "MG Form", Group = "MG Form" } },
            { 1036, new DocumentTypeInfo { DocumentType = "MG4F", Category = "MG Form", Group = "MG Form" } },
            { 1006, new DocumentTypeInfo { DocumentType = "MG5", Category = "MG Form", Group = "MG Form" } },
            { 1038, new DocumentTypeInfo { DocumentType = "MG5(SP)", Category = "MG Form", Group = "MG Form" } },
            { 1007, new DocumentTypeInfo { DocumentType = "MG6", Category = "MG Form", Group = "MG Form" } },
            { 1039, new DocumentTypeInfo { DocumentType = "MG6A", Category = "MG Form", Group = "MG Form" } },
            { 1008, new DocumentTypeInfo { DocumentType = "MG6B", Category = "MG Form", Group = "MG Form" } },
            { 1009, new DocumentTypeInfo { DocumentType = "MG6C", Category = "Always Unused", Group = "MG Form" } },
            { 1010, new DocumentTypeInfo { DocumentType = "MG6D", Category = "Always Unused", Group = "MG Form" } },
            { 1011, new DocumentTypeInfo { DocumentType = "MG6E", Category = "Always Unused", Group = "MG Form" } },
            { 1012, new DocumentTypeInfo { DocumentType = "MG7", Category = "MG Form", Group = "MG Form" } },
            { 1013, new DocumentTypeInfo { DocumentType = "MG8", Category = "MG Form", Group = "MG Form" } },
            { 1014, new DocumentTypeInfo { DocumentType = "MG9", Category = "MG Form", Group = "MG Form" } },
            { 1015, new DocumentTypeInfo { DocumentType = "MG10", Category = "MG Form", Group = "MG Form" } },
            { 1031, new DocumentTypeInfo { DocumentType = "MG11", Category = "Statement", Group = "Statement" } },
            { 1059, new DocumentTypeInfo { DocumentType = "MG11(R) or MG11(B)", Category = "MG Form" } },
            { 1019, new DocumentTypeInfo { DocumentType = "MG12", Category = "MG Form", Group = "MG Form" } },
            { 1040, new DocumentTypeInfo { DocumentType = "MG13", Category = "MG Form", Group = "MG Form" } },
            { 1041, new DocumentTypeInfo { DocumentType = "MG14", Category = "MG Form", Group = "MG Form" } },
            { 1042, new DocumentTypeInfo { DocumentType = "MG15(SDN)", Category = "Exhibit", Group = "Exhibit" } },
            { 1020, new DocumentTypeInfo { DocumentType = "MG15(ROTI)", Category = "Exhibit", Group = "Exhibit" } },
            { 1044, new DocumentTypeInfo { DocumentType = "MG15(ROVI)", Category = "Exhibit", Group = "Exhibit" } },
            { 1062, new DocumentTypeInfo { DocumentType = "MG15(CNOI)", Category = "Exhibit", Group = "Exhibit" } },
            { 1063, new DocumentTypeInfo { DocumentType = "MG16", Category = "MG Form", Group = "MG Form" } },
            { 1045, new DocumentTypeInfo { DocumentType = "MG16(DBCI)", Category = "MG Form", Group = "MG Form" } },
            { 1046, new DocumentTypeInfo { DocumentType = "MG16(D0I)", Category = "MG Form", Group = "MG Form" } },
            { 1047, new DocumentTypeInfo { DocumentType = "MG17", Category = "MG Form", Group = "MG Form" } },
            { 1024, new DocumentTypeInfo { DocumentType = "MG18", Category = "MG Form", Group = "MG Form" } },
            { 1025, new DocumentTypeInfo { DocumentType = "MG19", Category = "MG Form", Group = "MG Form" } },
            { 1026, new DocumentTypeInfo { DocumentType = "MG20", Category = "MG Form", Group = "MG Form" } },
            { 1048, new DocumentTypeInfo { DocumentType = "MG21", Category = "MG Form", Group = "MG Form" } },
            { 1049, new DocumentTypeInfo { DocumentType = "MG21A", Category = "MG Form", Group = "MG Form" } },
            { 1203, new DocumentTypeInfo { DocumentType = "MG22 SFR", Category = "MG Form", Group = "MG Form" } },
            { 1050, new DocumentTypeInfo { DocumentType = "MGDD", Category = "MG Form", Group = "MG Form" } },
            { 1027, new DocumentTypeInfo { DocumentType = "MG/FSSA", Category = "MG Form", Group = "MG Form" } },
            { 1028, new DocumentTypeInfo { DocumentType = "MG/FSSB", Category = "MG Form", Group = "MG Form" } },
            { 1029, new DocumentTypeInfo { DocumentType = "Other Communication", Category = "Communication", Group = "Other" } },
            { 1030, new DocumentTypeInfo { DocumentType = "Other Exhibit", Category = "Exhibit", Group = "Exhibit" } },
            { 1200, new DocumentTypeInfo { DocumentType = "Other Material", Category = "Other Material", Group = "Other" } },
            { 1056, new DocumentTypeInfo { DocumentType = "PCN1", Category = "Other Material", Group = "Other" } },
            { 1057, new DocumentTypeInfo { DocumentType = "PCN2", Category = "Other Material", Group = "Other" } },
            { 1058, new DocumentTypeInfo { DocumentType = "PCN3", Category = "Always Unused", Group = "Other" } },
            { 1051, new DocumentTypeInfo { DocumentType = "PE1", Category = "MG Form", Group = "MG Form" } },
            { 1052, new DocumentTypeInfo { DocumentType = "PE2", Category = "MG Form", Group = "MG Form" } },
            { 1053, new DocumentTypeInfo { DocumentType = "PE3", Category = "MG Form", Group = "MG Form" } },
            { 1054, new DocumentTypeInfo { DocumentType = "PE4", Category = "MG Form", Group = "MG Form" } },
            { 1202, new DocumentTypeInfo { DocumentType = "SDC Streamlined Disclosure Certificate", Category = "Always Unused", Group = "Other" } },
            { -3, new DocumentTypeInfo { DocumentType = "Service of ABE", Category = "Communication", Group = "Other" } },
            { -11, new DocumentTypeInfo { DocumentType = "Conference with Counsel", Category = "Communication" } },
            { -10, new DocumentTypeInfo { DocumentType = "CPS WCU Communication", Category = "Communication" } },
            { -9, new DocumentTypeInfo { DocumentType = "Meeting", Category = "Communication" } },
            { -8, new DocumentTypeInfo { DocumentType = "Physical Media", Category = "Communication" } },
            { 225887, new DocumentTypeInfo { DocumentType = "CC HRS", Category = "Case Overview" } },
            { 117, new DocumentTypeInfo { DocumentType = "AR01", Category = "Communication" } },
            { 118, new DocumentTypeInfo { DocumentType = "AR02", Category = "Communication" } },
            { 119, new DocumentTypeInfo { DocumentType = "AR03", Category = "Communication" } },
            { 120, new DocumentTypeInfo { DocumentType = "AR04", Category = "Communication" } },
            { 121, new DocumentTypeInfo { DocumentType = "AR05", Category = "Communication" } },
            { 122, new DocumentTypeInfo { DocumentType = "AR06", Category = "Communication" } },
            { 123, new DocumentTypeInfo { DocumentType = "AR07", Category = "Communication" } },
            { 124, new DocumentTypeInfo { DocumentType = "AR08", Category = "Communication" } },
            { 125, new DocumentTypeInfo { DocumentType = "AR09", Category = "Communication" } },
            { 126, new DocumentTypeInfo { DocumentType = "AR10", Category = "Communication" } },
            { 127, new DocumentTypeInfo { DocumentType = "AR11", Category = "Communication" } },
            { 128, new DocumentTypeInfo { DocumentType = "AR12", Category = "Communication" } },
            { 129, new DocumentTypeInfo { DocumentType = "AR13", Category = "Communication" } },
            { 130, new DocumentTypeInfo { DocumentType = "AR14", Category = "Communication" } },
            { 131, new DocumentTypeInfo { DocumentType = "AR15", Category = "Communication" } },
            { 132, new DocumentTypeInfo { DocumentType = "AR16", Category = "Communication" } },
            { 133, new DocumentTypeInfo { DocumentType = "AR17", Category = "Communication" } },
            { 134, new DocumentTypeInfo { DocumentType = "AR18", Category = "Communication" } },
            { 161, new DocumentTypeInfo { DocumentType = "ASW", Category = "Communication" } },
            { 152, new DocumentTypeInfo { DocumentType = "BCE01", Category = "Communication" } },
            { 153, new DocumentTypeInfo { DocumentType = "BCE02", Category = "Communication" } },
            { 195, new DocumentTypeInfo { DocumentType = "BCE03", Category = "Communication" } },
            { 224298, new DocumentTypeInfo { DocumentType = "BFS1", Category = "Communication" } },
            { 184, new DocumentTypeInfo { DocumentType = "CBO1", Category = "Communication" } },
            { 185, new DocumentTypeInfo { DocumentType = "CBO2", Category = "Communication" } },
            { 2, new DocumentTypeInfo { DocumentType = "CCA01", Category = "Communication" } },
            { 5, new DocumentTypeInfo { DocumentType = "CCCP09", Category = "Communication" } },
            { 6, new DocumentTypeInfo { DocumentType = "CCCP12", Category = "Communication" } },
            { 99, new DocumentTypeInfo { DocumentType = "CD1", Category = "Communication" } },
            { 162, new DocumentTypeInfo { DocumentType = "CMP", Category = "Communication" } },
            { 201, new DocumentTypeInfo { DocumentType = "CMPSCR", Category = "Communication" } },
            { 13, new DocumentTypeInfo { DocumentType = "CMS01", Category = "Communication" } },
            { 15, new DocumentTypeInfo { DocumentType = "CMS11", Category = "Communication" } },
            { 23, new DocumentTypeInfo { DocumentType = "CMS27", Category = "Communication" } },
            { 24, new DocumentTypeInfo { DocumentType = "CMS28", Category = "Communication" } },
            { 25, new DocumentTypeInfo { DocumentType = "CMS30", Category = "Communication" } },
            { 89, new DocumentTypeInfo { DocumentType = "CMS34P", Category = "Communication" } },
            { 91, new DocumentTypeInfo { DocumentType = "CMS36P", Category = "Communication" } },
            { 97, new DocumentTypeInfo { DocumentType = "CMS37L", Category = "Communication" } },
            { 98, new DocumentTypeInfo { DocumentType = "CMS38L", Category = "Communication" } },
            { 33, new DocumentTypeInfo { DocumentType = "CMS40", Category = "Communication" } },
            { 34, new DocumentTypeInfo { DocumentType = "CMS41", Category = "Communication" } },
            { 136, new DocumentTypeInfo { DocumentType = "CMS41b", Category = "Communication" } },
            { 35, new DocumentTypeInfo { DocumentType = "CMS42", Category = "Communication" } },
            { 36, new DocumentTypeInfo { DocumentType = "CMS43", Category = "Communication" } },
            { 42, new DocumentTypeInfo { DocumentType = "CMS49", Category = "Communication" } },
            { 46, new DocumentTypeInfo { DocumentType = "CMS53", Category = "Communication" } },
            { 105, new DocumentTypeInfo { DocumentType = "CMS55", Category = "Communication" } },
            { 111, new DocumentTypeInfo { DocumentType = "CMS56", Category = "Communication" } },
            { 112, new DocumentTypeInfo { DocumentType = "CMS57", Category = "Communication" } },
            { 174, new DocumentTypeInfo { DocumentType = "CMS61", Category = "Communication" } },
            { 205, new DocumentTypeInfo { DocumentType = "CP3-Crt", Category = "Communication" } },
            { 145, new DocumentTypeInfo { DocumentType = "CPIA01 (pre Apr05)", Category = "Communication" } },
            { 146, new DocumentTypeInfo { DocumentType = "CPIA02 (pre Apr05)", Category = "Communication" } },
            { 147, new DocumentTypeInfo { DocumentType = "CPIA03 (pre Apr05)", Category = "Communication" } },
            { 148, new DocumentTypeInfo { DocumentType = "CPIA04 (pre Apr05)", Category = "Communication" } },
            { 149, new DocumentTypeInfo { DocumentType = "CPIA05 (pre Apr05)", Category = "Communication" } },
            { 150, new DocumentTypeInfo { DocumentType = "CPIA06 (pre Apr05)", Category = "Communication" } },
            { 151, new DocumentTypeInfo { DocumentType = "CPIA07 (pre Apr05)", Category = "Communication" } },
            { 220, new DocumentTypeInfo { DocumentType = "CPIA1 (CC)", Category = "Communication" } },
            { 138, new DocumentTypeInfo { DocumentType = "CPIA1 (MC)", Category = "Communication" } },
            { 221, new DocumentTypeInfo { DocumentType = "CPIA2 (CC)", Category = "Communication" } },
            { 139, new DocumentTypeInfo { DocumentType = "CPIA2 (MC)", Category = "Communication" } },
            { 140, new DocumentTypeInfo { DocumentType = "CPIA3", Category = "Communication" } },
            { 141, new DocumentTypeInfo { DocumentType = "CPIA4", Category = "Communication" } },
            { 142, new DocumentTypeInfo { DocumentType = "CPIA4a", Category = "Communication" } },
            { 143, new DocumentTypeInfo { DocumentType = "CPIA5", Category = "Communication" } },
            { 144, new DocumentTypeInfo { DocumentType = "CPIA6", Category = "Communication" } },
            { 100232, new DocumentTypeInfo { DocumentType = "CPIA8", Category = "Communication" } },
            { 226015, new DocumentTypeInfo { DocumentType = "CPIA8", Category = "Communication" } },
            { 186, new DocumentTypeInfo { DocumentType = "CPR3", Category = "Communication" } },
            { 58, new DocumentTypeInfo { DocumentType = "CTL02", Category = "Communication" } },
            { 59, new DocumentTypeInfo { DocumentType = "CTL03", Category = "Communication" } },
            { 109, new DocumentTypeInfo { DocumentType = "CTL04", Category = "Communication" } },
            { 108, new DocumentTypeInfo { DocumentType = "CVR02", Category = "Communication" } },
            { 231, new DocumentTypeInfo { DocumentType = "DCF_BB_D", Category = "Communication" } },
            { 232, new DocumentTypeInfo { DocumentType = "DCF_BB_N", Category = "Communication" } },
            { 229, new DocumentTypeInfo { DocumentType = "DCF_FH_D", Category = "Communication" } },
            { 230, new DocumentTypeInfo { DocumentType = "DCF_FH_N", Category = "Communication" } },
            { 100230, new DocumentTypeInfo { DocumentType = "DDE1", Category = "Communication" } },
            { 225357, new DocumentTypeInfo { DocumentType = "DDE1", Category = "Communication" } },
            { 225638, new DocumentTypeInfo { DocumentType = "DDE3", Category = "Communication" } },
            { 206, new DocumentTypeInfo { DocumentType = "DEFRAINF", Category = "Communication" } },
            { 211, new DocumentTypeInfo { DocumentType = "DEFRALET", Category = "Communication" } },
            { 207, new DocumentTypeInfo { DocumentType = "DEFRALIC", Category = "Communication" } },
            { 208, new DocumentTypeInfo { DocumentType = "DEFRAREP", Category = "Communication" } },
            { 209, new DocumentTypeInfo { DocumentType = "DEFRAS9", Category = "Communication" } },
            { 210, new DocumentTypeInfo { DocumentType = "DEFRASUM", Category = "Communication" } },
            { 197, new DocumentTypeInfo { DocumentType = "DGCC", Category = "Communication" } },
            { 198, new DocumentTypeInfo { DocumentType = "DGYCC", Category = "Communication" } },
            { 222, new DocumentTypeInfo { DocumentType = "DIRCT", Category = "Communication" } },
            { 223, new DocumentTypeInfo { DocumentType = "DIRDEF", Category = "Communication" } },
            { 61, new DocumentTypeInfo { DocumentType = "DN01", Category = "Communication" } },
            { 63, new DocumentTypeInfo { DocumentType = "DN03", Category = "Communication" } },
            { 64, new DocumentTypeInfo { DocumentType = "DN04", Category = "Communication" } },
            { 225526, new DocumentTypeInfo { DocumentType = "DN06", Category = "Communication" } },
            { 202, new DocumentTypeInfo { DocumentType = "DN09", Category = "Communication" } },
            { 65, new DocumentTypeInfo { DocumentType = "DP01", Category = "Communication" } },
            { 137, new DocumentTypeInfo { DocumentType = "DRS", Category = "Communication" } },
            { 216, new DocumentTypeInfo { DocumentType = "DWPCPO", Category = "Communication" } },
            { 215, new DocumentTypeInfo { DocumentType = "DWPCPR", Category = "Communication" } },
            { 213, new DocumentTypeInfo { DocumentType = "DWPDISC1", Category = "Communication" } },
            { 214, new DocumentTypeInfo { DocumentType = "DWPDISC2", Category = "Communication" } },
            { 217, new DocumentTypeInfo { DocumentType = "DWPPR", Category = "Communication" } },
            { 67, new DocumentTypeInfo { DocumentType = "DWR01", Category = "Communication" } },
            { 226047, new DocumentTypeInfo { DocumentType = "EG2", Category = "Communication" } },
            { 225564, new DocumentTypeInfo { DocumentType = "EXP1", Category = "Communication" } },
            { 100240, new DocumentTypeInfo { DocumentType = "FEES01", Category = "Communication" } },
            { 225886, new DocumentTypeInfo { DocumentType = "FEES01", Category = "Communication" } },
            { 164, new DocumentTypeInfo { DocumentType = "HFCC1", Category = "Communication" } },
            { 228, new DocumentTypeInfo { DocumentType = "HR", Category = "Communication" } },
            { 224, new DocumentTypeInfo { DocumentType = "HRS-MC", Category = "Communication" } },
            { 226136, new DocumentTypeInfo { DocumentType = "IDPC1", Category = "Communication" } },
            { 190, new DocumentTypeInfo { DocumentType = "KDAR01", Category = "Communication" } },
            { 69, new DocumentTypeInfo { DocumentType = "MIC01", Category = "Communication" } },
            { 70, new DocumentTypeInfo { DocumentType = "NEA01", Category = "Communication" } },
            { 71, new DocumentTypeInfo { DocumentType = "NFE01", Category = "Communication" } },
            { 72, new DocumentTypeInfo { DocumentType = "NFE02", Category = "Communication" } },
            { 74, new DocumentTypeInfo { DocumentType = "NFE04", Category = "Communication" } },
            { 175, new DocumentTypeInfo { DocumentType = "NFR-CCF1", Category = "Communication" } },
            { 176, new DocumentTypeInfo { DocumentType = "NFR-CCF2", Category = "Communication" } },
            { 177, new DocumentTypeInfo { DocumentType = "NFR-CCF3", Category = "Communication" } },
            { 158, new DocumentTypeInfo { DocumentType = "NFR-HE1", Category = "Communication" } },
            { 159, new DocumentTypeInfo { DocumentType = "NFR-HE2", Category = "Communication" } },
            { 196, new DocumentTypeInfo { DocumentType = "NFR-HE3", Category = "Communication" } },
            { 219, new DocumentTypeInfo { DocumentType = "PCRWel", Category = "Communication" } },
            { 100235, new DocumentTypeInfo { DocumentType = "PFD01", Category = "Communication" } },
            { 225583, new DocumentTypeInfo { DocumentType = "PFD01", Category = "Communication" } },
            { 100234, new DocumentTypeInfo { DocumentType = "PFD02-", Category = "Communication" } },
            { 225584, new DocumentTypeInfo { DocumentType = "PFD02-", Category = "Communication" } },
            { 100236, new DocumentTypeInfo { DocumentType = "PFD03", Category = "Communication" } },
            { 225581, new DocumentTypeInfo { DocumentType = "PFD03", Category = "Communication" } },
            { 100233, new DocumentTypeInfo { DocumentType = "PFD04", Category = "Communication" } },
            { 225582, new DocumentTypeInfo { DocumentType = "PFD04", Category = "Communication" } },
            { 68, new DocumentTypeInfo { DocumentType = "POL1", Category = "Communication" } },
            { 203, new DocumentTypeInfo { DocumentType = "PRMB", Category = "Communication" } },
            { 78, new DocumentTypeInfo { DocumentType = "PSR", Category = "Communication" } },
            { 226570, new DocumentTypeInfo { DocumentType = "RASSO2", Category = "Communication" } },
            { 226650, new DocumentTypeInfo { DocumentType = "RASSO3", Category = "Communication" } },
            { 225595, new DocumentTypeInfo { DocumentType = "RCP02", Category = "Communication" } },
            { 225596, new DocumentTypeInfo { DocumentType = "RCP04", Category = "Communication" } },
            { 512, new DocumentTypeInfo { DocumentType = "RRA01", Category = "Communication" } },
            { 513, new DocumentTypeInfo { DocumentType = "RRA02", Category = "Communication" } },
            { 192, new DocumentTypeInfo { DocumentType = "s51(Ct)", Category = "Communication" } },
            { 193, new DocumentTypeInfo { DocumentType = "s51(Def)", Category = "Communication" } },
            { 80, new DocumentTypeInfo { DocumentType = "S9", Category = "Communication" } },
            { 100231, new DocumentTypeInfo { DocumentType = "STWAC01", Category = "Communication" } },
            { 225553, new DocumentTypeInfo { DocumentType = "STWAC01", Category = "Communication" } },
            { 225552, new DocumentTypeInfo { DocumentType = "SWTR01", Category = "Communication" } },
            { 165, new DocumentTypeInfo { DocumentType = "VHCC1", Category = "Communication" } },
            { 166, new DocumentTypeInfo { DocumentType = "VHCC2", Category = "Communication" } },
            { 167, new DocumentTypeInfo { DocumentType = "VHCC3", Category = "Communication" } },
            { 168, new DocumentTypeInfo { DocumentType = "VHCC4", Category = "Communication" } },
            { 169, new DocumentTypeInfo { DocumentType = "VHCC5", Category = "Communication" } },
            { 170, new DocumentTypeInfo { DocumentType = "VHCC6", Category = "Communication" } },
            { 171, new DocumentTypeInfo { DocumentType = "VHCC7", Category = "Communication" } },
            { 172, new DocumentTypeInfo { DocumentType = "VHCC8", Category = "Communication" } },
            { 173, new DocumentTypeInfo { DocumentType = "VHCC9", Category = "Communication" } },
            { 226594, new DocumentTypeInfo { DocumentType = "VictimLU", Category = "Communication" } },
            { 226598, new DocumentTypeInfo { DocumentType = "VictimLU5", Category = "Communication" } },
            { 514, new DocumentTypeInfo { DocumentType = "VIW01", Category = "Communication" } },
            { 88, new DocumentTypeInfo { DocumentType = "VIW05", Category = "Communication" } },
            { 226, new DocumentTypeInfo { DocumentType = "WIH01", Category = "Communication" } },
            { 225644, new DocumentTypeInfo { DocumentType = "AVT", Category = "Court Preparation" } },
            { 1503, new DocumentTypeInfo { DocumentType = "BCMFIN", Category = "Court Preparation" } },
            { 225590, new DocumentTypeInfo { DocumentType = "CFSBrief", Category = "Court Preparation" } },
            { 225254, new DocumentTypeInfo { DocumentType = "COTR1", Category = "Court Preparation" } },
            { 223239, new DocumentTypeInfo { DocumentType = "CTL05", Category = "Court Preparation" } },
            { 223240, new DocumentTypeInfo { DocumentType = "CTL06", Category = "Court Preparation" } },
            { 225545, new DocumentTypeInfo { DocumentType = "ITA", Category = "Court Preparation" } },
            { 227465, new DocumentTypeInfo { DocumentType = "LLA1", Category = "Court Preparation" } },
            { 226558, new DocumentTypeInfo { DocumentType = "MCPETv3", Category = "Court Preparation" } },
            { 516, new DocumentTypeInfo { DocumentType = "MG17", Category = "Court Preparation" } },
            { 1500, new DocumentTypeInfo { DocumentType = "PETFIN", Category = "Court Preparation" } },
            { 225627, new DocumentTypeInfo { DocumentType = "POLAW", Category = "Court Preparation" } },
            { 226497, new DocumentTypeInfo { DocumentType = "PSD1-a", Category = "Court Preparation" } },
            { 226379, new DocumentTypeInfo { DocumentType = "PTPH4", Category = "Court Preparation" } },
            { 225546, new DocumentTypeInfo { DocumentType = "RO", Category = "Court Preparation" } },
            { 187, new DocumentTypeInfo { DocumentType = "VIW11", Category = "Court Preparation" } },
            { 82, new DocumentTypeInfo { DocumentType = "WEF04", Category = "Court Preparation" } },
            { 100239, new DocumentTypeInfo { DocumentType = "EG3", Category = "Exhibit" } },
            { 226148, new DocumentTypeInfo { DocumentType = "EG3", Category = "Exhibit" } },
            { 227, new DocumentTypeInfo { DocumentType = "CAP01", Category = "Review" } },
            { 101, new DocumentTypeInfo { DocumentType = "MG3", Category = "Review" } },
            { 102, new DocumentTypeInfo { DocumentType = "MG3A", Category = "Review" } },
            { 104, new DocumentTypeInfo { DocumentType = "MG3AS", Category = "Review" } },
            { 103, new DocumentTypeInfo { DocumentType = "MG3S", Category = "Review" } },
            { 189, new DocumentTypeInfo { DocumentType = "REV1", Category = "Review" } },
            { 212, new DocumentTypeInfo { DocumentType = "REVIEW", Category = "Review" } },
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<DocumentTypeGroup> GetDocumentTypesWithClassificationGroup()
    {
        try
        {
            var result = new List<DocumentTypeGroup>();

            foreach (KeyValuePair<int, DocumentTypeInfo> item in this.documentTypeMapping.Where(x => x.Value.Group != null).OrderBy(x => x.Value.Group))
            {
                result.Add(new DocumentTypeGroup
                {
                    Id = item.Key,
                    Name = item.Value.DocumentType,
                    Group = item.Value.Group ?? string.Empty,
                    Category = item.Value.Category,
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching document types.");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public DocumentTypeInfo MapDocumentType(int documentTypeId)
    {
        return this.documentTypeMapping.TryGetValue(documentTypeId, out DocumentTypeInfo? info)
            ? info
            : new DocumentTypeInfo { DocumentType = "Unknown", Category = "Unknown" };
    }

    /// <inheritdoc />
    public DocumentTypeInfo MapMaterialType(string materialType)
    {
        // Convert the string to an int (if possible)
        if (int.TryParse(materialType, out int materialTypeId))
        {
            return this.documentTypeMapping.TryGetValue(materialTypeId, out DocumentTypeInfo? info)
                ? info
                : new DocumentTypeInfo { DocumentType = "Unknown", Category = "Unknown" };
        }
        else
        {
            // If the conversion fails, return a default value
            return new DocumentTypeInfo { DocumentType = "Unknown", Category = "Unknown" };
        }
    }
}
