import { RedactionSavePage } from "./RedactionSavePage";

export type RedactionSaveRequest = {
  docId: number;
  redactions: RedactionSavePage[];
};
