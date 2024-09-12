import { UrnLookupDataSource } from "./types/UrnLookupDataSource";

const dataSource: UrnLookupDataSource = (caseId: number) => {
  const caseIdAsString = caseId.toString();
  const urnRoot = `45CV29112${caseIdAsString.substring(
    caseIdAsString.length - 2
  )}`;
  const urn = `${urnRoot}/2`;
  return {
    id: 13401, // so that we go to our mock record
    urnRoot,
    urn,
  };
};

export default dataSource;
