//update save data type based on the data contract
export type ReclassifySaveData = {
  documentNewName?: string;
  documentUsedStatus: "YES" | "NO";
};
