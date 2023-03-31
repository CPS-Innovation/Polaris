import { RedactionSavePage } from "./RedactionSavePage";

export type RedactionSaveRequest = {
  documentId: string;
  redactions: RedactionSavePage[];
};
