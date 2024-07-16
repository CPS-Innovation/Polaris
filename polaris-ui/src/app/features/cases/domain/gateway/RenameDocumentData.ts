export type RenameDocumentData = {
  documentId: string;
  value: string;
  saveRenameStatus: "failure" | "saving" | "success" | "initial";
};
