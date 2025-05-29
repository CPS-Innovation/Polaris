export type UsedOrUnusedDocument = {
  documentId: string;
  saveStatus: "initial" | "saving" | "success" | "failure" | string;
  saveRefreshStatus: "initial" | "updating" | "updated";
};
