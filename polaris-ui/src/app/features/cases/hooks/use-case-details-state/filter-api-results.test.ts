import { ApiTextSearchResult } from "../../domain/gateway/ApiTextSearchResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { filterApiResults } from "./filter-api-results";

describe("filter-api-results", () => {
  it("can only return items from api text search that are present in the current document state", () => {
    const apiResults = [
      { documentId: "1" },
      { documentId: "2" },
      { documentId: "3" },
    ] as ApiTextSearchResult[];

    const documentState = [
      { documentId: "2" },
      { documentId: "3" },
      { documentId: "4" },
    ] as MappedCaseDocument[];

    const result = filterApiResults(apiResults, documentState);

    expect(result).toEqual([
      { documentId: "2" },
      { documentId: "3" },
    ] as ApiTextSearchResult[]);
  });
});
