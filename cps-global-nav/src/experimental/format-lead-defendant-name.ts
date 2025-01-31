import { CaseDetails } from "./domain/CaseDetails";

export const formatLeadDefendantName = (item: CaseDetails) => {
  if (!item.leadDefendantDetails) {
    return null;
  }

  let titleString =
    item.leadDefendantDetails.type === "Organisation"
      ? item.leadDefendantDetails.organisationName
      : `${item.leadDefendantDetails.surname}, ${item.leadDefendantDetails.firstNames}`;

  if (item.numberOfDefendants > 1) {
    titleString = `${titleString} and others`;
  }
  return titleString;
};
