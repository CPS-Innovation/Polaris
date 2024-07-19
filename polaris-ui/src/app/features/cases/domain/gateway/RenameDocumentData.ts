export type RenameDocumentData = {
  documentId: string;
  newName: string;
  saveRenameStatus: "failure" | "saving" | "success" | "initial";
  saveRenameRefreshStatus: "initial" | "updating" | "updated";
};
