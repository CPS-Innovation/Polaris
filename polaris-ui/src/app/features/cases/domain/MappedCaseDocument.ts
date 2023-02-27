import { CaseDocument } from "./CaseDocument";

export type MappedCaseDocument = CaseDocument & {
  presentationCategory: string;
  presentationFileName: string;
};
