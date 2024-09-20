import { RedactionTypeData } from "../domain/redactionLog/RedactionLogData";
export type PageDeleteRedaction = {
  pageNumber: number;
  redactionType: RedactionTypeData;
};
export interface IPageDeleteRedaction extends PageDeleteRedaction {
  id: string;
}
