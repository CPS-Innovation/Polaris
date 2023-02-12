import { RedactionSavePage } from "./RedactionSavePage";

export type RedactionSaveRequest = {
  docId: string;
  redactions: RedactionSavePage[];
};
