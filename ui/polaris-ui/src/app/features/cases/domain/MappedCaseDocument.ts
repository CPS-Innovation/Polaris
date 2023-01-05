import { CaseDocument } from "./CaseDocument";

export type MappedCaseDocument = CaseDocument & {
  tabSafeId: string;
  presentationCategory: string;
  presentationFileName: string;
};
