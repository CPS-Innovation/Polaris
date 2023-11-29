import { IPdfHighlight } from "./IPdfHighlight";
import { MappedCaseDocument } from "./MappedCaseDocument";
import { SavingStatus } from "./gateway/SavingStatus";
export type CaseDocumentViewModel = MappedCaseDocument & {
  savingStatus: SavingStatus;
  isDeleted: boolean;
  url: string | undefined;
  pdfBlobName: string | undefined;
  sasUrl: undefined | string;
  redactionHighlights: IPdfHighlight[];
  clientLockedState: // note: unlocked is just the state where the client doesn't know yet
  //  (might be locked on the server, we haven't interacted yet)
  "unlocked" | "locking" | "locked" | "unlocking" | "locked-by-other-user";
} & (
    | { mode: "read" }
    | {
        mode: "search";
        searchTerm: string;
        occurrencesInDocumentCount: number;
        searchHighlights: IPdfHighlight[];
      }
  );
