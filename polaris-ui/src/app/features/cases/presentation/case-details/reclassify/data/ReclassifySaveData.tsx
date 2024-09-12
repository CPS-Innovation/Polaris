export type ReclassifySaveData = {
  documentId: number;
  documentTypeId: number;
  immediate: {
    documentName: string | null;
  } | null;
  other: {
    documentName: string | null;
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
    newProducer: string | null;
    item: string;
    reference: string;
    used: boolean;
  } | null;
};
