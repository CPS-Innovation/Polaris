import { PresentationDocumentProperties } from "../../app/features/cases/domain/gateway/PipelineDocument";
import { DocumentsListDataSource } from "./types/DocumentsListDataSource";

const documentsList: PresentationDocumentProperties[] = [
  {
    documentId: "testid",
    versionId: 1,
    cmsOriginalFileName: "MCLOVEMG3.pdf",
    presentationTitle: "Test item ",
    cmsFileCreatedDate: "2022-06-02T21:22:33Z",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1029,
      documentType: "MG3",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "Ok",
      write: "Ok",
    },
    parentDocumentId: "4",
    witnessId: null,
    hasFailedAttachments: true,
    hasNotes: true,
    isUnused: true,
    isInbox: false,
    isOcrProcessed: false,
    classification: null,
    isWitnessManagement: false,
    canReclassify: true,
    canRename: true,
    renameStatus: "CanRename",
    reference: "Reference 1",
  },
  {
    documentId: "2",
    versionId: 1,
    cmsOriginalFileName: "CM01.pdf",
    presentationTitle: "CM01 Item 4 very long",
    cmsFileCreatedDate: "2020-06-02",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 1029,
      documentType: "MG11",
      documentCategory: "Communication",
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
    reference: "Reference 2",
  },
  {
    documentId: "CMS-3333",
    versionId: 1,
    cmsOriginalFileName: "MG05MCLOVE.pdf",
    presentationTitle: "MG05MCLOVE very long",
    cmsFileCreatedDate: "2020-06-02",
    categoryListOrder: null,
    cmsDocType: {
      documentTypeId: 5,
      documentType: "MG5",
      documentCategory: "MGForm",
    },
    presentationFlags: {
      read: "Ok",
      write: "Ok",
    },
    parentDocumentId: "4",
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
];

const getDocumentsListResult = (resultsCount: number) => {
  let resultsArray = Array(resultsCount)
    .fill([])
    .map((value, index) =>
      documentsList.map((document) => ({
        ...document,
        versionId: document.versionId + index,
      }))
    );
  return resultsArray;
};

const dataSource: DocumentsListDataSource = getDocumentsListResult(5);

export default dataSource;
