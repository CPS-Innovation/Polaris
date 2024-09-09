//update save data type based on the data contract
export type ReclassifySaveData = {
  documentId: string;
  documentTypeId: number;
  immediate: {
    newTitle: string | null;
  } | null;
  other: {
    newTitle: string | null;
    used: boolean;
  } | null;
  statement: {
    witnessId: number;
    statementNo: number;
    date: string;
    used: boolean;
  } | null;
  exhibit: {
    existingProducerOrWitnessId: number | null;
    newProducer: string;
    item: string;
    reference: string;
    used: boolean;
  } | null;
};
