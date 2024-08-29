//update save data type based on the data contract
export type ReclassifySaveData = {
  documentId: string;
  documentTypeId: number;
  reclassificationType: "Statement" | "Exhibit" | "Other" | "Immediate";
  statement: {
    witnessId: number;
    statementNo: number;
    date: string;
  };
  exhibit: {
    existingProducerOrWitnessId: number | null;
    newProducer: string;
    item: string;
    reference: string;
  };
  used: boolean;
};
