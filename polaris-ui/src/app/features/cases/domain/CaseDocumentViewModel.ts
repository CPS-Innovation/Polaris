import { IPdfHighlight } from "./IPdfHighlight";
import { MappedCaseDocument } from "./MappedCaseDocument";
import { IPageDeleteRedaction } from "./IPageDeleteRedaction";
import { SaveStatus } from "./gateway/SaveStatus";
export type CaseDocumentViewModel = MappedCaseDocument & {
  saveStatus: SaveStatus;
  isDeleted: boolean;
  url: string | undefined;
  pdfBlobName: string | undefined;
  sasUrl: undefined | string;
  areaOnlyRedactionMode: boolean;
  redactionHighlights: IPdfHighlight[];
  pageDeleteRedactions: IPageDeleteRedaction[];
  clientLockedState: // note: unlocked is just the state where the client doesn't know yet
  //  (might be locked on the server, we haven't interacted yet)
  ClientLockedState;
} & (
    | { mode: "read" }
    | {
        mode: "search";
        searchTerm: string;
        occurrencesInDocumentCount: number;
        searchHighlights: IPdfHighlight[];
      }
  );

export type ClientLockedState =
  | "unlocked"
  | "locking"
  | "locked"
  | "unlocking"
  | "locked-by-other-user";
