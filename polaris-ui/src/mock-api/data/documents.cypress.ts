import { CaseDocument } from "../../app/features/cases/domain/CaseDocument";
import { DocumentsDataSource } from "./types/DocumentsDataSource";

const dataSource: DocumentsDataSource = (id: number) => documents;

export default dataSource;

const documents: CaseDocument[] = [
  {
    documentId: 1,
    fileName: "MCLOVEMG3",
    createdDate: "2020-06-01",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 1,
      code: "MG11",
      name: "MG11 File",
    },
  },
  {
    documentId: 2,
    fileName: "CM01",
    createdDate: "2020-06-02",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 2,
      code: "MG12",
      name: "MG12 File",
    },
  },
  {
    documentId: 3,
    fileName: "MG05MCLOVE",
    createdDate: "2020-06-03",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 3,
      code: "MG13",
      name: "MG13 File",
    },
  },
  {
    documentId: 4,
    fileName: "MG06_3June",
    createdDate: "2020-06-04",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 4,
      code: "MG14",
      name: "MG14 File",
    },
  },
  {
    documentId: 5,
    fileName: "MG06_10june",
    createdDate: "2020-06-10",
    cmsDocCategory: "MGForm",
    cmsDocType: {
      id: 5,
      code: "MG15",
      name: "MG15 File",
    },
  },
];
