import { CmsDocCategory } from "./CmsDocCategory";

export type CmsDocType = {
  documentTypeId: number | null;
  documentType: string;
  documentCategory: CmsDocCategory;
};
