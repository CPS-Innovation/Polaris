import { RedactionSaveCoordinates } from "./RedactionSaveCoordinates";

export type RedactionSavePage = {
  pageIndex: number;
  height: number;
  width: number;
  redactionCoordinates: RedactionSaveCoordinates[];
};
