import { CaseDetails } from "../../../domain/CaseDetails";

export const checkMultipleDefendantsOrCharges = (
  caseDetails: CaseDetails
): boolean => {
  const { defendants } = caseDetails;
  if (defendants.length > 1) {
    return true;
  }

  const { charges } = defendants[0];
  if (charges.length > 1) {
    return true;
  }
  return false;
};
