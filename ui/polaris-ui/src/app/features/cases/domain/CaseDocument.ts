import { CmsDocCategory } from "./CmsDocCategory";

export type CaseDocument = {
  documentId: number;
  fileName: string;
  createdDate: string;
  cmsDocCategory: CmsDocCategory;
  cmsDocType: {
    id: number;
    code: string;
    name: string;
  };
};
