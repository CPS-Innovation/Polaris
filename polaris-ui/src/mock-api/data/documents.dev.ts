import { CaseDocument } from "../../app/features/cases/domain/CaseDocument";
import { DocumentsDataSource } from "./types/DocumentsDataSource";

const dataSource: DocumentsDataSource = (id: number) => documents;

export default dataSource;

const documents: CaseDocument[] = [
  {
    documentId: 1,
    fileName: "MCLOVEMG3  very long .docx",
    createdDate: "2020-06-02",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 3,
      code: "MG3",
      name: "MG3 File",
    },
  },
  {
    documentId: 2,
    fileName: "CM01  very long .docx",
    createdDate: "2020-06-02",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 11,
      code: "MG11",
      name: "MG11 File",
    },
  },
  {
    documentId: 3,
    fileName: "MG05MCLOVE very long .docx",
    createdDate: "2020-06-02",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 5,
      code: "MG5",
      name: "MG5 File",
    },
  },
  {
    documentId: 4,
    fileName: "MG06_3June  very long .docx",
    createdDate: "2020-06-03",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 6,
      code: "MG6",
      name: "MG6 File",
    },
  },
  {
    documentId: 5,
    fileName: "MG06_10june  very long .docx",
    createdDate: "2020-06-10",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 3,
      code: "MG3",
      name: "MG3 File",
    },
  },
  {
    documentId: 6,
    fileName: "MCLOVEMG3  very long .docx",
    createdDate: "2020-06-02",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 3,
      code: "MG3",
      name: "MG3 File",
    },
  },
  {
    documentId: 7,
    fileName: "CM01  very long .docx",
    createdDate: "2020-06-02",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: -1,
      code: "Other Comm (In)",
      name: "Other Comm (In) File",
    },
  },
  {
    documentId: 8,
    fileName: "MG05MCLOVE very long .docx",
    createdDate: "2020-06-02",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 5,
      code: "MG5",
      name: "MG5 File",
    },
  },
  {
    documentId: 9,
    fileName: "MG06_3June  very long .docx",
    createdDate: "2020-06-03",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 6,
      code: "MG6",
      name: "MG6 File",
    },
  },
  {
    documentId: 10,
    fileName: "MG06_10june  very long .docx",
    createdDate: "2020-06-10",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 6,
      code: "MG6",
      name: "MG6 File",
    },
  },
];
