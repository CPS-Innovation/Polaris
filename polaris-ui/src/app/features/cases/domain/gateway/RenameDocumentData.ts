export type RenameDocumentData = {
  documentId: string;
  saveRenameStatus: "failure" | "saving" | "success" | "initial";
  // saveRenameRefreshStatus: "initial" | "updating" | "updated";
};
