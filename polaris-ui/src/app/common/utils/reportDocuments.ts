const reportedDocuments: string[] = [];

export const isAlreadyReportedDocument = (documentId: string) => {
  const isAlreadyReported = !!reportedDocuments.find((id) => id === documentId);
  return isAlreadyReported;
};

export const addToReportedDocuments = (documentId: string) => {
  if (isAlreadyReportedDocument(documentId)) return;
  reportedDocuments.push(documentId);
};
