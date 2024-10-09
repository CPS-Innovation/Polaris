export type RenameDocumentData = {
  documentId: string;
  newName: string;
  saveRenameStatus: "initial" | "saving" | "success" | "failure";
  saveRenameRefreshStatus: "initial" | "updating" | "updated";
};
