const reportedDocuments: string[] = [];

export const isAlreadyReportedDocument = (documentId: string) => {
  console.log("reportedDocuments>>>", reportedDocuments);
  const isAlreadyReported = !!reportedDocuments.find((id) => id === documentId);
  console.log("isAlreadyReported>>>", isAlreadyReported);
  return isAlreadyReported;
};

export const addToReportedDocuments = (documentId: string) => {
  if (isAlreadyReportedDocument(documentId)) return;
  reportedDocuments.push(documentId);
};
