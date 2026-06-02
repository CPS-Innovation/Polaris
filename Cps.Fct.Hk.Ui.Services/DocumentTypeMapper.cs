// <copyright file="DocumentTypeMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System.Collections.Generic;
using Common.Constants;
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
            { 1201, new DocumentTypeInfo { DocumentType = "ABE", Category = DocumentTypeCategories.OtherMaterial, Group = DocumentTypeGroups.Other } },
            { -7, new DocumentTypeInfo { DocumentType = "Asset Recovery", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { -6, new DocumentTypeInfo { DocumentType = "Asset Recovery 3rd Party", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { -5, new DocumentTypeInfo { DocumentType = "Complaint", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { -4, new DocumentTypeInfo { DocumentType = "Counsel Case Acknowledgement ", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { 1064, new DocumentTypeInfo { DocumentType = "DCF", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.MgForm } },
            { 1032, new DocumentTypeInfo { DocumentType = "Forwarded Internal Email", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { -1, new DocumentTypeInfo { DocumentType = "Correspondence", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { 1055, new DocumentTypeInfo { DocumentType = "DREP", Category = DocumentTypeCategories.Communication } },
            { -2, new DocumentTypeInfo { DocumentType = "Defence Statement", Category = DocumentTypeCategories.OtherMaterial, Group = DocumentTypeGroups.Other } },
            { 1066, new DocumentTypeInfo { DocumentType = "MG00", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1001, new DocumentTypeInfo { DocumentType = "MG1 (no longer received)", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1033, new DocumentTypeInfo { DocumentType = "MG2", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1034, new DocumentTypeInfo { DocumentType = "MG3", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1035, new DocumentTypeInfo { DocumentType = "MG3A", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1002, new DocumentTypeInfo { DocumentType = "MG4", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1003, new DocumentTypeInfo { DocumentType = "MG4A", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1004, new DocumentTypeInfo { DocumentType = "MG4B", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1005, new DocumentTypeInfo { DocumentType = "MG4C", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1060, new DocumentTypeInfo { DocumentType = "MG4D", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1061, new DocumentTypeInfo { DocumentType = "MG4E", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1037, new DocumentTypeInfo { DocumentType = "MG4(Post)", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1036, new DocumentTypeInfo { DocumentType = "MG4F", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1006, new DocumentTypeInfo { DocumentType = "MG5", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1038, new DocumentTypeInfo { DocumentType = "MG5(SP)", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1007, new DocumentTypeInfo { DocumentType = "MG6", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1039, new DocumentTypeInfo { DocumentType = "MG6A", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1008, new DocumentTypeInfo { DocumentType = "MG6B", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1009, new DocumentTypeInfo { DocumentType = "MG6C", Category = DocumentTypeCategories.UnusedMaterial, Group = DocumentTypeGroups.MgForm } },
            { 1010, new DocumentTypeInfo { DocumentType = "MG6D", Category = DocumentTypeCategories.UnusedMaterial, Group = DocumentTypeGroups.MgForm } },
            { 1011, new DocumentTypeInfo { DocumentType = "MG6E", Category = DocumentTypeCategories.UnusedMaterial, Group = DocumentTypeGroups.MgForm } },
            { 1012, new DocumentTypeInfo { DocumentType = "MG7", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1013, new DocumentTypeInfo { DocumentType = "MG8", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1014, new DocumentTypeInfo { DocumentType = "MG9", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1015, new DocumentTypeInfo { DocumentType = "MG10", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1031, new DocumentTypeInfo { DocumentType = "MG11", Category = DocumentTypeCategories.Statement, Group = DocumentTypeGroups.Statement } },
            { 1059, new DocumentTypeInfo { DocumentType = "MG11(R) or MG11(B)", Category = DocumentTypeCategories.Statement } },
            { 1019, new DocumentTypeInfo { DocumentType = "MG12", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1040, new DocumentTypeInfo { DocumentType = "MG13", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1041, new DocumentTypeInfo { DocumentType = "MG14", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1042, new DocumentTypeInfo { DocumentType = "MG15(SDN)", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1020, new DocumentTypeInfo { DocumentType = "MG15(ROTI)", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1044, new DocumentTypeInfo { DocumentType = "MG15(ROVI)", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1062, new DocumentTypeInfo { DocumentType = "MG15(CNOI)", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1063, new DocumentTypeInfo { DocumentType = "MG16", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1045, new DocumentTypeInfo { DocumentType = "MG16(DBCI)", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1046, new DocumentTypeInfo { DocumentType = "MG16(D0I)", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1047, new DocumentTypeInfo { DocumentType = "MG17", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1024, new DocumentTypeInfo { DocumentType = "MG18", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1025, new DocumentTypeInfo { DocumentType = "MG19", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1026, new DocumentTypeInfo { DocumentType = "MG20", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1048, new DocumentTypeInfo { DocumentType = "MG21", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1049, new DocumentTypeInfo { DocumentType = "MG21A", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1203, new DocumentTypeInfo { DocumentType = "MG22 SFR", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1050, new DocumentTypeInfo { DocumentType = "MGDD", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1027, new DocumentTypeInfo { DocumentType = "MG/FSSA", Category = DocumentTypeCategories.MgForm, Group = DocumentTypeGroups.MgForm } },
            { 1028, new DocumentTypeInfo { DocumentType = "MG/FSSB", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1029, new DocumentTypeInfo { DocumentType = "Other Communication", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { 1030, new DocumentTypeInfo { DocumentType = "Other Exhibit", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1200, new DocumentTypeInfo { DocumentType = "Other Material", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { 1056, new DocumentTypeInfo { DocumentType = "PCN1", Category = DocumentTypeCategories.DefendantPreCons, Group = DocumentTypeGroups.Other } },
            { 1057, new DocumentTypeInfo { DocumentType = "PCN2", Category = DocumentTypeCategories.DefendantPreCons, Group = DocumentTypeGroups.Other } },
            { 1058, new DocumentTypeInfo { DocumentType = "PCN3", Category = DocumentTypeCategories.UnusedMaterial, Group = DocumentTypeGroups.MgForm } },
            { 1051, new DocumentTypeInfo { DocumentType = "PE1", Category = DocumentTypeCategories.Statement, Group = DocumentTypeGroups.Statement } },
            { 1052, new DocumentTypeInfo { DocumentType = "PE2", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1053, new DocumentTypeInfo { DocumentType = "PE3", Category = DocumentTypeCategories.Communication } },
            { 1054, new DocumentTypeInfo { DocumentType = "PE4", Category = DocumentTypeCategories.Communication } },
            { 1202, new DocumentTypeInfo { DocumentType = "SDC Streamlined Disclosure Certificate", Category = DocumentTypeCategories.UnusedMaterial, Group = DocumentTypeGroups.MgForm } },
            { -3, new DocumentTypeInfo { DocumentType = "Service of ABE", Category = DocumentTypeCategories.Communication, Group = DocumentTypeGroups.Other } },
            { -11, new DocumentTypeInfo { DocumentType = "Conference with Counsel", Category = DocumentTypeCategories.Communication } },
            { -10, new DocumentTypeInfo { DocumentType = "CPS WCU Communication", Category = DocumentTypeCategories.Communication } },
            { -9, new DocumentTypeInfo { DocumentType = "Meeting", Category = DocumentTypeCategories.Communication } },
            { -8, new DocumentTypeInfo { DocumentType = "Physical Media", Category = DocumentTypeCategories.Communication } },
            { 225887, new DocumentTypeInfo { DocumentType = "CC HRS", Category = DocumentTypeCategories.CaseOverview } },
            { 117, new DocumentTypeInfo { DocumentType = "AR01", Category = DocumentTypeCategories.Communication } },
            { 118, new DocumentTypeInfo { DocumentType = "AR02", Category = DocumentTypeCategories.Communication } },
            { 119, new DocumentTypeInfo { DocumentType = "AR03", Category = DocumentTypeCategories.Communication } },
            { 120, new DocumentTypeInfo { DocumentType = "AR04", Category = DocumentTypeCategories.Communication } },
            { 121, new DocumentTypeInfo { DocumentType = "AR05", Category = DocumentTypeCategories.Communication } },
            { 122, new DocumentTypeInfo { DocumentType = "AR06", Category = DocumentTypeCategories.Communication } },
            { 123, new DocumentTypeInfo { DocumentType = "AR07", Category = DocumentTypeCategories.Communication } },
            { 124, new DocumentTypeInfo { DocumentType = "AR08", Category = DocumentTypeCategories.Communication } },
            { 125, new DocumentTypeInfo { DocumentType = "AR09", Category = DocumentTypeCategories.Communication } },
            { 126, new DocumentTypeInfo { DocumentType = "AR10", Category = DocumentTypeCategories.Communication } },
            { 127, new DocumentTypeInfo { DocumentType = "AR11", Category = DocumentTypeCategories.Communication } },
            { 128, new DocumentTypeInfo { DocumentType = "AR12", Category = DocumentTypeCategories.Communication } },
            { 129, new DocumentTypeInfo { DocumentType = "AR13", Category = DocumentTypeCategories.Communication } },
            { 130, new DocumentTypeInfo { DocumentType = "AR14", Category = DocumentTypeCategories.Communication } },
            { 131, new DocumentTypeInfo { DocumentType = "AR15", Category = DocumentTypeCategories.Communication } },
            { 132, new DocumentTypeInfo { DocumentType = "AR16", Category = DocumentTypeCategories.Communication } },
            { 133, new DocumentTypeInfo { DocumentType = "AR17", Category = DocumentTypeCategories.Communication } },
            { 134, new DocumentTypeInfo { DocumentType = "AR18", Category = DocumentTypeCategories.Communication } },
            { 161, new DocumentTypeInfo { DocumentType = "ASW", Category = DocumentTypeCategories.Communication } },
            { 152, new DocumentTypeInfo { DocumentType = "BCE01", Category = DocumentTypeCategories.Communication } },
            { 153, new DocumentTypeInfo { DocumentType = "BCE02", Category = DocumentTypeCategories.Communication } },
            { 195, new DocumentTypeInfo { DocumentType = "BCE03", Category = DocumentTypeCategories.Communication } },
            { 224298, new DocumentTypeInfo { DocumentType = "BFS1", Category = DocumentTypeCategories.Communication } },
            { 184, new DocumentTypeInfo { DocumentType = "CBO1", Category = DocumentTypeCategories.Communication } },
            { 185, new DocumentTypeInfo { DocumentType = "CBO2", Category = DocumentTypeCategories.Communication } },
            { 2, new DocumentTypeInfo { DocumentType = "CCA01", Category = DocumentTypeCategories.Communication } },
            { 5, new DocumentTypeInfo { DocumentType = "CCCP09", Category = DocumentTypeCategories.Communication } },
            { 6, new DocumentTypeInfo { DocumentType = "CCCP12", Category = DocumentTypeCategories.Communication } },
            { 99, new DocumentTypeInfo { DocumentType = "CD1", Category = DocumentTypeCategories.Communication } },
            { 162, new DocumentTypeInfo { DocumentType = "CMP", Category = DocumentTypeCategories.Communication } },
            { 201, new DocumentTypeInfo { DocumentType = "CMPSCR", Category = DocumentTypeCategories.Communication } },
            { 13, new DocumentTypeInfo { DocumentType = "CMS01", Category = DocumentTypeCategories.Communication } },
            { 15, new DocumentTypeInfo { DocumentType = "CMS11", Category = DocumentTypeCategories.Communication } },
            { 23, new DocumentTypeInfo { DocumentType = "CMS27", Category = DocumentTypeCategories.Communication } },
            { 24, new DocumentTypeInfo { DocumentType = "CMS28", Category = DocumentTypeCategories.Communication } },
            { 25, new DocumentTypeInfo { DocumentType = "CMS30", Category = DocumentTypeCategories.Communication } },
            { 89, new DocumentTypeInfo { DocumentType = "CMS34P", Category = DocumentTypeCategories.Communication } },
            { 91, new DocumentTypeInfo { DocumentType = "CMS36P", Category = DocumentTypeCategories.Communication } },
            { 97, new DocumentTypeInfo { DocumentType = "CMS37L", Category = DocumentTypeCategories.Communication } },
            { 98, new DocumentTypeInfo { DocumentType = "CMS38L", Category = DocumentTypeCategories.Communication } },
            { 33, new DocumentTypeInfo { DocumentType = "CMS40", Category = DocumentTypeCategories.Communication } },
            { 34, new DocumentTypeInfo { DocumentType = "CMS41", Category = DocumentTypeCategories.Communication } },
            { 136, new DocumentTypeInfo { DocumentType = "CMS41b", Category = DocumentTypeCategories.Communication } },
            { 35, new DocumentTypeInfo { DocumentType = "CMS42", Category = DocumentTypeCategories.Communication } },
            { 36, new DocumentTypeInfo { DocumentType = "CMS43", Category = DocumentTypeCategories.Communication } },
            { 42, new DocumentTypeInfo { DocumentType = "CMS49", Category = DocumentTypeCategories.Communication } },
            { 46, new DocumentTypeInfo { DocumentType = "CMS53", Category = DocumentTypeCategories.Communication } },
            { 105, new DocumentTypeInfo { DocumentType = "CMS55", Category = DocumentTypeCategories.Communication } },
            { 111, new DocumentTypeInfo { DocumentType = "CMS56", Category = DocumentTypeCategories.Communication } },
            { 112, new DocumentTypeInfo { DocumentType = "CMS57", Category = DocumentTypeCategories.Communication } },
            { 174, new DocumentTypeInfo { DocumentType = "CMS61", Category = DocumentTypeCategories.Communication } },
            { 205, new DocumentTypeInfo { DocumentType = "CP3-Crt", Category = DocumentTypeCategories.Communication } },
            { 145, new DocumentTypeInfo { DocumentType = "CPIA01 (pre Apr05)", Category = DocumentTypeCategories.Communication } },
            { 146, new DocumentTypeInfo { DocumentType = "CPIA02 (pre Apr05)", Category = DocumentTypeCategories.Communication } },
            { 147, new DocumentTypeInfo { DocumentType = "CPIA03 (pre Apr05)", Category = DocumentTypeCategories.Communication } },
            { 148, new DocumentTypeInfo { DocumentType = "CPIA04 (pre Apr05)", Category = DocumentTypeCategories.Communication } },
            { 149, new DocumentTypeInfo { DocumentType = "CPIA05 (pre Apr05)", Category = DocumentTypeCategories.Communication } },
            { 150, new DocumentTypeInfo { DocumentType = "CPIA06 (pre Apr05)", Category = DocumentTypeCategories.Communication } },
            { 151, new DocumentTypeInfo { DocumentType = "CPIA07 (pre Apr05)", Category = DocumentTypeCategories.Communication } },
            { 220, new DocumentTypeInfo { DocumentType = "CPIA1 (CC)", Category = DocumentTypeCategories.Communication } },
            { 138, new DocumentTypeInfo { DocumentType = "CPIA1 (MC)", Category = DocumentTypeCategories.Communication } },
            { 221, new DocumentTypeInfo { DocumentType = "CPIA2 (CC)", Category = DocumentTypeCategories.Communication } },
            { 139, new DocumentTypeInfo { DocumentType = "CPIA2 (MC)", Category = DocumentTypeCategories.Communication } },
            { 140, new DocumentTypeInfo { DocumentType = "CPIA3", Category = DocumentTypeCategories.Communication } },
            { 141, new DocumentTypeInfo { DocumentType = "CPIA4", Category = DocumentTypeCategories.Communication } },
            { 142, new DocumentTypeInfo { DocumentType = "CPIA4a", Category = DocumentTypeCategories.Communication } },
            { 143, new DocumentTypeInfo { DocumentType = "CPIA5", Category = DocumentTypeCategories.Communication } },
            { 144, new DocumentTypeInfo { DocumentType = "CPIA6", Category = DocumentTypeCategories.Communication } },
            { 100232, new DocumentTypeInfo { DocumentType = "CPIA8", Category = DocumentTypeCategories.Communication } },
            { 226015, new DocumentTypeInfo { DocumentType = "CPIA8", Category = DocumentTypeCategories.Communication } },
            { 186, new DocumentTypeInfo { DocumentType = "CPR3", Category = DocumentTypeCategories.Communication } },
            { 58, new DocumentTypeInfo { DocumentType = "CTL02", Category = DocumentTypeCategories.Communication } },
            { 59, new DocumentTypeInfo { DocumentType = "CTL03", Category = DocumentTypeCategories.Communication } },
            { 109, new DocumentTypeInfo { DocumentType = "CTL04", Category = DocumentTypeCategories.Communication } },
            { 108, new DocumentTypeInfo { DocumentType = "CVR02", Category = DocumentTypeCategories.Communication } },
            { 231, new DocumentTypeInfo { DocumentType = "DCF_BB_D", Category = DocumentTypeCategories.Communication } },
            { 232, new DocumentTypeInfo { DocumentType = "DCF_BB_N", Category = DocumentTypeCategories.Communication } },
            { 229, new DocumentTypeInfo { DocumentType = "DCF_FH_D", Category = DocumentTypeCategories.Communication } },
            { 230, new DocumentTypeInfo { DocumentType = "DCF_FH_N", Category = DocumentTypeCategories.Communication } },
            { 100230, new DocumentTypeInfo { DocumentType = "DDE1", Category = DocumentTypeCategories.Communication } },
            { 225357, new DocumentTypeInfo { DocumentType = "DDE1", Category = DocumentTypeCategories.Communication } },
            { 225638, new DocumentTypeInfo { DocumentType = "DDE3", Category = DocumentTypeCategories.Communication } },
            { 206, new DocumentTypeInfo { DocumentType = "DEFRAINF", Category = DocumentTypeCategories.Communication } },
            { 211, new DocumentTypeInfo { DocumentType = "DEFRALET", Category = DocumentTypeCategories.Communication } },
            { 207, new DocumentTypeInfo { DocumentType = "DEFRALIC", Category = DocumentTypeCategories.Communication } },
            { 208, new DocumentTypeInfo { DocumentType = "DEFRAREP", Category = DocumentTypeCategories.Communication } },
            { 209, new DocumentTypeInfo { DocumentType = "DEFRAS9", Category = DocumentTypeCategories.Communication } },
            { 210, new DocumentTypeInfo { DocumentType = "DEFRASUM", Category = DocumentTypeCategories.Communication } },
            { 197, new DocumentTypeInfo { DocumentType = "DGCC", Category = DocumentTypeCategories.Communication } },
            { 198, new DocumentTypeInfo { DocumentType = "DGYCC", Category = DocumentTypeCategories.Communication } },
            { 222, new DocumentTypeInfo { DocumentType = "DIRCT", Category = DocumentTypeCategories.Communication } },
            { 223, new DocumentTypeInfo { DocumentType = "DIRDEF", Category = DocumentTypeCategories.Communication } },
            { 61, new DocumentTypeInfo { DocumentType = "DN01", Category = DocumentTypeCategories.Communication } },
            { 63, new DocumentTypeInfo { DocumentType = "DN03", Category = DocumentTypeCategories.Communication } },
            { 64, new DocumentTypeInfo { DocumentType = "DN04", Category = DocumentTypeCategories.Communication } },
            { 225526, new DocumentTypeInfo { DocumentType = "DN06", Category = DocumentTypeCategories.Communication } },
            { 202, new DocumentTypeInfo { DocumentType = "DN09", Category = DocumentTypeCategories.Communication } },
            { 65, new DocumentTypeInfo { DocumentType = "DP01", Category = DocumentTypeCategories.Communication } },
            { 137, new DocumentTypeInfo { DocumentType = "DRS", Category = DocumentTypeCategories.Communication } },
            { 216, new DocumentTypeInfo { DocumentType = "DWPCPO", Category = DocumentTypeCategories.Communication } },
            { 215, new DocumentTypeInfo { DocumentType = "DWPCPR", Category = DocumentTypeCategories.Communication } },
            { 213, new DocumentTypeInfo { DocumentType = "DWPDISC1", Category = DocumentTypeCategories.Communication } },
            { 214, new DocumentTypeInfo { DocumentType = "DWPDISC2", Category = DocumentTypeCategories.Communication } },
            { 217, new DocumentTypeInfo { DocumentType = "DWPPR", Category = DocumentTypeCategories.Communication } },
            { 67, new DocumentTypeInfo { DocumentType = "DWR01", Category = DocumentTypeCategories.Communication } },
            { 226047, new DocumentTypeInfo { DocumentType = "EG2", Category = DocumentTypeCategories.Communication } },
            { 225564, new DocumentTypeInfo { DocumentType = "EXP1", Category = DocumentTypeCategories.Communication } },
            { 100240, new DocumentTypeInfo { DocumentType = "FEES01", Category = DocumentTypeCategories.Communication } },
            { 225886, new DocumentTypeInfo { DocumentType = "FEES01", Category = DocumentTypeCategories.Communication } },
            { 164, new DocumentTypeInfo { DocumentType = "HFCC1", Category = DocumentTypeCategories.Communication } },
            { 228, new DocumentTypeInfo { DocumentType = "HR", Category = DocumentTypeCategories.Communication } },
            { 224, new DocumentTypeInfo { DocumentType = "HRS-MC", Category = DocumentTypeCategories.Communication } },
            { 226136, new DocumentTypeInfo { DocumentType = "IDPC1", Category = DocumentTypeCategories.Communication } },
            { 190, new DocumentTypeInfo { DocumentType = "KDAR01", Category = DocumentTypeCategories.Communication } },
            { 69, new DocumentTypeInfo { DocumentType = "MIC01", Category = DocumentTypeCategories.Communication } },
            { 70, new DocumentTypeInfo { DocumentType = "NEA01", Category = DocumentTypeCategories.Communication } },
            { 71, new DocumentTypeInfo { DocumentType = "NFE01", Category = DocumentTypeCategories.Communication } },
            { 72, new DocumentTypeInfo { DocumentType = "NFE02", Category = DocumentTypeCategories.Communication } },
            { 74, new DocumentTypeInfo { DocumentType = "NFE04", Category = DocumentTypeCategories.Communication } },
            { 175, new DocumentTypeInfo { DocumentType = "NFR-CCF1", Category = DocumentTypeCategories.Communication } },
            { 176, new DocumentTypeInfo { DocumentType = "NFR-CCF2", Category = DocumentTypeCategories.Communication } },
            { 177, new DocumentTypeInfo { DocumentType = "NFR-CCF3", Category = DocumentTypeCategories.Communication } },
            { 158, new DocumentTypeInfo { DocumentType = "NFR-HE1", Category = DocumentTypeCategories.Communication } },
            { 159, new DocumentTypeInfo { DocumentType = "NFR-HE2", Category = DocumentTypeCategories.Communication } },
            { 196, new DocumentTypeInfo { DocumentType = "NFR-HE3", Category = DocumentTypeCategories.Communication } },
            { 219, new DocumentTypeInfo { DocumentType = "PCRWel", Category = DocumentTypeCategories.Communication } },
            { 100235, new DocumentTypeInfo { DocumentType = "PFD01", Category = DocumentTypeCategories.Communication } },
            { 225583, new DocumentTypeInfo { DocumentType = "PFD01", Category = DocumentTypeCategories.Communication } },
            { 100234, new DocumentTypeInfo { DocumentType = "PFD02-", Category = DocumentTypeCategories.Communication } },
            { 225584, new DocumentTypeInfo { DocumentType = "PFD02-", Category = DocumentTypeCategories.Communication } },
            { 100236, new DocumentTypeInfo { DocumentType = "PFD03", Category = DocumentTypeCategories.Communication } },
            { 225581, new DocumentTypeInfo { DocumentType = "PFD03", Category = DocumentTypeCategories.Communication } },
            { 100233, new DocumentTypeInfo { DocumentType = "PFD04", Category = DocumentTypeCategories.Communication } },
            { 225582, new DocumentTypeInfo { DocumentType = "PFD04", Category = DocumentTypeCategories.Communication } },
            { 68, new DocumentTypeInfo { DocumentType = "POL1", Category = DocumentTypeCategories.Communication } },
            { 203, new DocumentTypeInfo { DocumentType = "PRMB", Category = DocumentTypeCategories.Communication } },
            { 78, new DocumentTypeInfo { DocumentType = "PSR", Category = DocumentTypeCategories.Communication } },
            { 226570, new DocumentTypeInfo { DocumentType = "RASSO2", Category = DocumentTypeCategories.Communication } },
            { 226650, new DocumentTypeInfo { DocumentType = "RASSO3", Category = DocumentTypeCategories.Communication } },
            { 225595, new DocumentTypeInfo { DocumentType = "RCP02", Category = DocumentTypeCategories.Communication } },
            { 225596, new DocumentTypeInfo { DocumentType = "RCP04", Category = DocumentTypeCategories.Communication } },
            { 512, new DocumentTypeInfo { DocumentType = "RRA01", Category = DocumentTypeCategories.Communication } },
            { 513, new DocumentTypeInfo { DocumentType = "RRA02", Category = DocumentTypeCategories.Communication } },
            { 192, new DocumentTypeInfo { DocumentType = "s51(Ct)", Category = DocumentTypeCategories.Communication } },
            { 193, new DocumentTypeInfo { DocumentType = "s51(Def)", Category = DocumentTypeCategories.Communication } },
            { 80, new DocumentTypeInfo { DocumentType = "S9", Category = DocumentTypeCategories.Communication } },
            { 100231, new DocumentTypeInfo { DocumentType = "STWAC01", Category = DocumentTypeCategories.Communication } },
            { 225553, new DocumentTypeInfo { DocumentType = "STWAC01", Category = DocumentTypeCategories.Communication } },
            { 225552, new DocumentTypeInfo { DocumentType = "SWTR01", Category = DocumentTypeCategories.Communication } },
            { 165, new DocumentTypeInfo { DocumentType = "VHCC1", Category = DocumentTypeCategories.Communication } },
            { 166, new DocumentTypeInfo { DocumentType = "VHCC2", Category = DocumentTypeCategories.Communication } },
            { 167, new DocumentTypeInfo { DocumentType = "VHCC3", Category = DocumentTypeCategories.Communication } },
            { 168, new DocumentTypeInfo { DocumentType = "VHCC4", Category = DocumentTypeCategories.Communication } },
            { 169, new DocumentTypeInfo { DocumentType = "VHCC5", Category = DocumentTypeCategories.Communication } },
            { 170, new DocumentTypeInfo { DocumentType = "VHCC6", Category = DocumentTypeCategories.Communication } },
            { 171, new DocumentTypeInfo { DocumentType = "VHCC7", Category = DocumentTypeCategories.Communication } },
            { 172, new DocumentTypeInfo { DocumentType = "VHCC8", Category = DocumentTypeCategories.Communication } },
            { 173, new DocumentTypeInfo { DocumentType = "VHCC9", Category = DocumentTypeCategories.Communication } },
            { 226594, new DocumentTypeInfo { DocumentType = "VictimLU", Category = DocumentTypeCategories.Communication } },
            { 226598, new DocumentTypeInfo { DocumentType = "VictimLU5", Category = DocumentTypeCategories.Communication } },
            { 514, new DocumentTypeInfo { DocumentType = "VIW01", Category = DocumentTypeCategories.Communication } },
            { 88, new DocumentTypeInfo { DocumentType = "VIW05", Category = DocumentTypeCategories.Communication } },
            { 226, new DocumentTypeInfo { DocumentType = "WIH01", Category = DocumentTypeCategories.Communication } },
            { 225644, new DocumentTypeInfo { DocumentType = "AVT", Category = DocumentTypeCategories.CourtPreparation } },
            { 1503, new DocumentTypeInfo { DocumentType = "BCMFIN", Category = DocumentTypeCategories.CourtPreparation } },
            { 225590, new DocumentTypeInfo { DocumentType = "CFSBrief", Category = DocumentTypeCategories.CourtPreparation } },
            { 225254, new DocumentTypeInfo { DocumentType = "COTR1", Category = DocumentTypeCategories.CourtPreparation } },
            { 223239, new DocumentTypeInfo { DocumentType = "CTL05", Category = DocumentTypeCategories.CourtPreparation } },
            { 223240, new DocumentTypeInfo { DocumentType = "CTL06", Category = DocumentTypeCategories.CourtPreparation } },
            { 225545, new DocumentTypeInfo { DocumentType = "ITA", Category = DocumentTypeCategories.CourtPreparation } },
            { 227465, new DocumentTypeInfo { DocumentType = "LLA1", Category = DocumentTypeCategories.CourtPreparation } },
            { 226558, new DocumentTypeInfo { DocumentType = "MCPETv3", Category = DocumentTypeCategories.CourtPreparation } },
            { 516, new DocumentTypeInfo { DocumentType = "MG17", Category = DocumentTypeCategories.CourtPreparation } },
            { 1500, new DocumentTypeInfo { DocumentType = "PETFIN", Category = DocumentTypeCategories.CourtPreparation } },
            { 225627, new DocumentTypeInfo { DocumentType = "POLAW", Category = DocumentTypeCategories.CourtPreparation } },
            { 226497, new DocumentTypeInfo { DocumentType = "PSD1-a", Category = DocumentTypeCategories.CourtPreparation } },
            { 226379, new DocumentTypeInfo { DocumentType = "PTPH4", Category = DocumentTypeCategories.CourtPreparation } },
            { 225546, new DocumentTypeInfo { DocumentType = "RO", Category = DocumentTypeCategories.CourtPreparation } },
            { 187, new DocumentTypeInfo { DocumentType = "VIW11", Category = DocumentTypeCategories.CourtPreparation } },
            { 82, new DocumentTypeInfo { DocumentType = "WEF04", Category = DocumentTypeCategories.CourtPreparation } },
            { 100239, new DocumentTypeInfo { DocumentType = "EG3", Category = DocumentTypeCategories.UnusedMaterial } },
            { 226148, new DocumentTypeInfo { DocumentType = "EG3", Category = DocumentTypeCategories.UnusedMaterial } },
            { 227, new DocumentTypeInfo { DocumentType = "CAP01", Category = DocumentTypeCategories.Communication } },
            { 101, new DocumentTypeInfo { DocumentType = "MG3", Category = DocumentTypeCategories.Communication } },
            { 102, new DocumentTypeInfo { DocumentType = "MG3A", Category = DocumentTypeCategories.Communication } },
            { 104, new DocumentTypeInfo { DocumentType = "MG3AS", Category = DocumentTypeCategories.Communication } },
            { 103, new DocumentTypeInfo { DocumentType = "MG3S", Category = DocumentTypeCategories.Communication } },
            { 189, new DocumentTypeInfo { DocumentType = "REV1", Category = DocumentTypeCategories.Communication } },
            { 212, new DocumentTypeInfo { DocumentType = "REVIEW", Category = DocumentTypeCategories.Communication } },
            { 1016, new DocumentTypeInfo { DocumentType = "MG 11(CONT)", Category = DocumentTypeCategories.Statement, Group = DocumentTypeGroups.Statement } },
            { 1017, new DocumentTypeInfo { DocumentType = "MG 11(T)", Category = DocumentTypeCategories.Statement, Group = DocumentTypeGroups.Statement } },
            { 1018, new DocumentTypeInfo { DocumentType = "MG 11T(CONT)", Category = DocumentTypeCategories.Statement, Group = DocumentTypeGroups.Statement } },
            { 1021, new DocumentTypeInfo { DocumentType = "MG 15(CONT)", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1022, new DocumentTypeInfo { DocumentType = "MG 15(T)",       Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1023, new DocumentTypeInfo { DocumentType = "MG 15(T)(CONT)", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 1406, new DocumentTypeInfo { DocumentType = "ABE Recording", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
            { 225522, new DocumentTypeInfo { DocumentType = "999Wav", Category = DocumentTypeCategories.Exhibit, Group = DocumentTypeGroups.Exhibit } },
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
