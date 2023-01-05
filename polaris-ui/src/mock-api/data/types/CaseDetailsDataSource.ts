import { CaseDetails } from "../../../app/features/cases/domain/CaseDetails";

export const lastRequestedUrnCache = {
  urn: "",
};

export type CaseDetailsDataSource = (id: number) => CaseDetails | undefined;
