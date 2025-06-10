import { PresentationDocumentProperties } from "../../app/features/cases/domain/gateway/PipelineDocument";
import { DocumentsListDataSource } from "./types/DocumentsListDataSource";

const documentsList: PresentationDocumentProperties[] = [
  {
    documentId: "1",
    cmsOriginalFileName: "MCLOVEMG3.pdf",
    presentationTitle: "MCLOVEMG3",
    cmsFileCreatedDate: "2020-06-01",
    versionId: 1,
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1,
      documentType: "MG11",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "Ok",
      write: "Ok",
    },
    parentDocumentId: "4",
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: true,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: true,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "2",
    versionId: 1,
    cmsOriginalFileName: "CM01.pdf",
    presentationTitle: "CM01",
    cmsFileCreatedDate: "2020-06-02",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1019,
      documentType: "MG12",
      documentCategory: "Exhibit",
    },
    presentationFlags: {
      read: "Ok",
      write: "DocTypeNotAllowed",
    },
    parentDocumentId: "4",
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: true,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: true,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "3",
    versionId: 1,
    cmsOriginalFileName: "MG05MCLOVE.pdf",
    presentationTitle: "Doc_3",
    cmsFileCreatedDate: "2020-06-03",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1012,
      documentType: "MG13",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "OnlyAvailableInCms",
      write: "Ok",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: false,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "4",
    versionId: 1,
    cmsOriginalFileName: "MG06_3June.hte",
    presentationTitle: "Doc_4",
    cmsFileCreatedDate: "2020-06-04",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 4,
      documentType: "MG14",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "Ok",
      write: "Ok",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "5",
    versionId: 1,
    cmsOriginalFileName: "MG06_10june.hte",
    presentationTitle: "Doc_5",
    cmsFileCreatedDate: "2020-06-10",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 5,
      documentType: "MG15",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "Ok",
      write: "OriginalFileTypeNotAllowed",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "6",
    versionId: 1,
    cmsOriginalFileName: "CM01.pdf",
    presentationTitle: "Test DAC",
    cmsFileCreatedDate: "2023-05-11",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1029,
      documentType: "DAC",
      documentCategory: "Review",
    },
    presentationFlags: {
      read: "Ok",
      write: "DocTypeNotAllowed",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "8",
    versionId: 1,
    cmsOriginalFileName: "MG06_3June.pdf",
    presentationTitle: "Doc_8",
    cmsFileCreatedDate: "2020-06-04",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 4,
      documentType: "MG14",
      documentCategory: "Review",
    },
    presentationFlags: {
      read: "Ok",
      write: "OnlyAvailableInCms",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "9",
    versionId: 1,
    cmsOriginalFileName: "MG06_3June.pdf",
    presentationTitle: "Doc_9",
    cmsFileCreatedDate: "2020-06-10",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 7,
      documentType: "MG15",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "Ok",
      write: "IsDispatched",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "10",
    versionId: 1,
    cmsOriginalFileName: "PortraitLandscape.pdf",
    presentationTitle: "PortraitLandscape",
    cmsFileCreatedDate: "2020-06-02",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1019,
      documentType: "MG12",
      documentCategory: "Exhibit",
    },
    presentationFlags: {
      read: "Ok",
      write: "Ok",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: true,
    canRename: true,
    renameStatus: "CanRename",
    reference: null,
  },
  {
    documentId: "12",
    versionId: 1,
    cmsOriginalFileName: "SearchPII.pdf",
    presentationTitle: "SearchPII",
    cmsFileCreatedDate: "2020-06-02",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1019,
      documentType: "MG12",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "Ok",
      write: "Ok",
    },
    parentDocumentId: null,
    witnessId: null,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: false,
    renameStatus: "IsStatement",
    reference: null,
  },
  {
    documentId: "13",
    versionId: 1,
    cmsOriginalFileName: "CM01.pdf",
    presentationTitle: "PCD Document",
    cmsFileCreatedDate: "2020-06-02",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1031,
      documentType: "PCD",
      documentCategory: "Review",
    },
    presentationFlags: {
      read: "Ok",
      write: "Ok",
    },
    parentDocumentId: null,
    witnessId: 2762766,
    hasFailedAttachments: false,
    hasNotes: false,
    isUnused: false,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: false,
    canRename: false,
    renameStatus: "CanRename",
    reference: null,
  },
];

export const getDocumentsListResult = (resultsCount: number) => {
  const resultsArray = Array(resultsCount)
    .fill({})
    .map((value, index) =>
      documentsList.map(
        (document) =>
          ({
            ...document,
            versionId: document.versionId + index,
          } as PresentationDocumentProperties)
      )
    );
  return resultsArray;
};

// This will create two results one with document id 2 and the second with document id 2 deleted.
export const getRefreshDeletedDocuments = (
  redactedDocumentId: string,
  deletedDocumentId: string,
  trackerCalls = 1
) => {
  const resultsArray = getRefreshRedactedDocument(
    redactedDocumentId,
    trackerCalls
  );
  return resultsArray.map((result, index) => {
    const filteredDocs = resultsArray[index].filter(
      ({ documentId }) => documentId !== deletedDocumentId
    );

    return [...filteredDocs];
  });
};

export const getRefreshRenamedDocuments = (
  id: string,
  newName: string,
  trackerCalls: number
): PresentationDocumentProperties[][] => {
  const resultsArray = getDocumentsListResult(trackerCalls);

  const filteredDocs = resultsArray[trackerCalls - 1].filter(
    ({ documentId }) => documentId !== id
  );
  const selectedDoc = resultsArray[trackerCalls - 1].find(
    ({ documentId }) => documentId === id
  )!;
  return resultsArray.map((result, index) => {
    return [...filteredDocs, { ...selectedDoc, presentationTitle: newName }];
  });
};

export const getUsedUnusedDocumentState = (
  id: string,
  isUnused: boolean,
  trackerCalls: number
) => {
  const resultsArray = getDocumentsListResult(trackerCalls);

  const filteredDocs = resultsArray[trackerCalls - 1].filter(
    ({ documentId }) => documentId !== id
  );
  const selectedDoc = resultsArray[trackerCalls - 1].find(
    ({ documentId }) => documentId === id
  )!;
  return resultsArray.map((result, index) => {
    return [
      ...filteredDocs,
      {
        ...selectedDoc,
        isUnused,
      },
    ];
  });
};

export const getRefreshReclassifyDocuments = (
  id: string,
  newDocTypeId: number,
  trackerCalls: number
): PresentationDocumentProperties[][] => {
  const resultsArray = getDocumentsListResult(trackerCalls);

  const filteredDocs = resultsArray[trackerCalls - 1].filter(
    ({ documentId }) => documentId !== id
  );
  const selectedDoc = resultsArray[trackerCalls - 1].find(
    ({ documentId }) => documentId === id
  )!;
  return resultsArray.map((result, index) => {
    return [
      ...filteredDocs,
      {
        ...selectedDoc,
        cmsDocType: {
          ...selectedDoc?.cmsDocType,
          documentTypeId: newDocTypeId,
        },
      },
    ];
  });
};

export const getRefreshRedactedDocument = (
  id: string,
  trackerCalls = 1
): PresentationDocumentProperties[][] => {
  const resultsArray = getDocumentsListResult(trackerCalls);

  return resultsArray.map((result, index) => {
    const filteredDocs = resultsArray[index].filter(
      ({ documentId }) => documentId !== id
    );
    const selectedDoc = resultsArray[index].find(
      ({ documentId }) => documentId === id
    )!;
    return [
      ...filteredDocs,
      { ...selectedDoc, versionId: selectedDoc.versionId + 1 },
    ];
  });
};
/* This is the documentList for notification testing which contains a new document, 
    a renamed document, a new version document and 
    a discarded document(deleted)to cover all the types of notifications
*/
export const getRefreshedDocumentsForNotification = () => {
  const resultsArray = getDocumentsListResult(2);
  const newDocument = {
    documentId: "15",
    canReclassify: false,
    canRename: false,
    categoryListOrder: null,
    classification: null,
    cmsDocType: {
      documentTypeId: 1012,
      documentType: "MG13",
      documentCategory: "MGForm",
    },
    cmsFileCreatedDate: "2020-06-03",
    cmsOriginalFileName: "MG05MCLOVE.pdf",

    hasFailedAttachments: false,
    hasNotes: false,
    isInbox: false,
    isOcrProcessed: false,
    isUnused: false,
    isWitnessManagement: false,
    parentDocumentId: null,
    presentationFlags: { read: "OnlyAvailableInCms", write: "Ok" },
    presentationTitle: "New Document_1",
    reference: null,
    renameStatus: "CanRename",
    versionId: 1,
    witnessId: null,
  };

  const documents = resultsArray[0];
  const newDocuments = [...documents, newDocument]
    .filter((document) => document.documentId !== "13")
    .map((document) => {
      switch (document.documentId) {
        case "1":
        case "3":
          return { ...document, versionId: document.versionId + 1 };
        case "2":
          return {
            ...document,
            presentationTitle: `${document.presentationTitle}_1`,
          };
        default:
          return document;
      }
    });

  return [documents, newDocuments];
};

// export const missingDocsPipelinePdfResults: PipelineResults = {
//   status: "Completed",
//   processingCompleted: new Date().toISOString(),
//   documentsRetrieved: new Date().toISOString(),
//   documents: [
//     {
//       documentId: "1",
//       versionId: 1,
//       status: "Indexed",
//       cmsOriginalFileName: "MCLOVEMG3.pdf",
//       presentationTitle: "MCLOVEMG3  very long",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 3,
//         documentType: "MG3",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: true,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "2",
//       versionId: 2,
//       status: "Indexed",
//       cmsOriginalFileName: "CM01.pdf",
//       presentationTitle: "CM01  very long",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 11,
//         documentType: "MG11",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: true,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "3",
//       versionId: 3,
//       status: "Indexed",
//       cmsOriginalFileName: "MG05MCLOVE.pdf",
//       presentationTitle: "MG05MCLOVE very long",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 5,
//         documentType: "MG5",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "4",
//       versionId: 4,
//       status: "OcrAndIndexFailure",
//       cmsOriginalFileName: "MG06_3June.pdf",
//       presentationTitle: "MG06_3June  very long",
//       cmsFileCreatedDate: "2020-06-03",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 6,
//         documentType: "MG6",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "5",
//       versionId: 5,
//       status: "UnableToConvertToPdf",
//       cmsOriginalFileName: "MG06_10june.pdf",
//       presentationTitle: "MG06_10june  very long",
//       cmsFileCreatedDate: "2020-06-10",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 3,
//         documentType: "MG3",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "12",
//       versionId: 12,
//       status: "Indexed",
//       cmsOriginalFileName: "SearchPII.pdf",
//       presentationTitle: "SearchPII",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 1019,
//         documentType: "MG12",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//   ],
// };

// export const allMissingDocsPipelinePdfResults: PipelineResults = {
//   status: "Completed",
//   processingCompleted: new Date().toISOString(),
//   documentsRetrieved: new Date().toISOString(),
//   documents: [
//     {
//       documentId: "1",
//       versionId: 1,
//       status: "OcrAndIndexFailure",
//       cmsOriginalFileName: "MCLOVEMG3.pdf",
//       presentationTitle: "MCLOVEMG3  very long",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 3,
//         documentType: "MG3",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: true,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "2",
//       versionId: 2,
//       status: "OcrAndIndexFailure",
//       cmsOriginalFileName: "CM01.pdf",
//       presentationTitle: "CM01  very long",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 11,
//         documentType: "MG11",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: true,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "3",
//       versionId: 3,
//       status: "OcrAndIndexFailure",
//       cmsOriginalFileName: "MG05MCLOVE.pdf",
//       presentationTitle: "MG05MCLOVE very long",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 5,
//         documentType: "MG5",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "4",
//       versionId: 4,
//       status: "OcrAndIndexFailure",
//       cmsOriginalFileName: "MG06_3June.pdf",
//       presentationTitle: "MG06_3June  very long",
//       cmsFileCreatedDate: "2020-06-03",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 6,
//         documentType: "MG6",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "5",
//       versionId: 5,
//       status: "UnableToConvertToPdf",
//       cmsOriginalFileName: "MG06_10june.pdf",
//       presentationTitle: "MG06_10june  very long",
//       cmsFileCreatedDate: "2020-06-10",

//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 3,
//         documentType: "MG3",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//     {
//       documentId: "12",
//       versionId: 12,
//       status: "Indexed",
//       cmsOriginalFileName: "SearchPII.pdf",
//       presentationTitle: "SearchPII",
//       cmsFileCreatedDate: "2020-06-02",
//       categoryListOrder: null,
//       cmsDocType: {
//         documentTypeId: 1019,
//         documentType: "MG12",
//         documentCategory: "MGForm",
//       },
//       presentationFlags: {
//         read: "Ok",
//         write: "Ok",
//       },
//       parentDocumentId: null,
//       witnessId: null,
//       hasFailedAttachments: false,
//       hasNotes: false,
//       conversionStatus: "DocumentConverted",
//       isUnused: false,
//       isInbox: false,
//       isOcrProcessed: false,
//       classification: null,
//       isWitnessManagement: false,
//       canReclassify: false,
//       canRename: false,
//       renameStatus: "CanRename",
//       reference: null,
//     },
//   ],
// };

// export const refreshPipelineDeletedDocuments: (
//   redactedDocumentId: string,
//   deletedDocumentId: string,
//   trackerCalls?: number
// ) => DocumentsListDataSource = (
//   redactedDocumentId,
//   deletedDocumentId,
//   trackerCalls
// ) => getRefreshDeletedDocuments(redactedDocumentId, deletedDocumentId);

// export const refreshPipelineRenamedDocuments: (
//   documentId: string,
//   newName: string,
//   trackerCalls: number
// ) => DocumentsListDataSource = (documentId, newName, trackerCalls) =>
//   getRefreshRenamedDocuments(documentId, newName, trackerCalls);

// export const refreshPipelineReclassifyDocuments: (
//   documentId: string,
//   newDocTypeId: number,
//   trackerCalls: number
// ) => DocumentsListDataSource = (documentId, newDocTypeId, trackerCalls) =>
//   getRefreshReclassifyDocuments(documentId, newDocTypeId, trackerCalls);

const dataSource: DocumentsListDataSource = getDocumentsListResult(9);
export default dataSource;
