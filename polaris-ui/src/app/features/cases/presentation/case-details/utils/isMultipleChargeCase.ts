import { CaseDetails } from "../../../domain/CaseDetails";

export const isMultipleChargeCase = (caseDetails: CaseDetails): boolean => {
  const { defendants } = caseDetails;

  return defendants.length > 1 || defendants[0]?.charges.length > 1;
};
