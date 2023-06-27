import { CmsDocCategory } from "./CmsDocCategory";

export type CmsDocType = {
  documentTypeId: number;
  documentType: string;
  documentCategory: CmsDocCategory;
};
