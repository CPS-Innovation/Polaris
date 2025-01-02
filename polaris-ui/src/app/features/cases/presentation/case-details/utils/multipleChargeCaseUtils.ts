import { CaseDetails } from "../../../domain/gateway/CaseDetails";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";

export const isMultipleChargeCase = (caseDetails: CaseDetails): boolean => {
  const { defendants } = caseDetails;

  return defendants.length > 1 || defendants[0]?.charges.length > 1;
};

export const getDACDocumentId = (documents: MappedCaseDocument[]): string => {
  const dacDocument = documents.find(
    (document) => document.cmsDocType.documentType === "DAC"
  );
  return dacDocument ? dacDocument.documentId : "";
};
