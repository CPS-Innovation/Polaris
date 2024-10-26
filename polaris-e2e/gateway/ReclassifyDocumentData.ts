export type ReclassifyDocumentData = {
  documentId: string;
  newDocTypeId: number;
  reclassified: boolean;
  saveReclassifyRefreshStatus: "initial" | "updating" | "updated";
};
