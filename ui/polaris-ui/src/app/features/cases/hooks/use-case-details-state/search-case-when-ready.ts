import { searchCase } from "../../api/gateway-api";

export const searchCaseWhenReady = async (
  urn: string,
  id: number,
  searchTerm: string,
  isPipelineComplete: boolean,
  isDocumentCallComplete: boolean
) => {
  return searchTerm && isPipelineComplete && isDocumentCallComplete
    ? await searchCase(urn, id, searchTerm)
    : undefined;
};
