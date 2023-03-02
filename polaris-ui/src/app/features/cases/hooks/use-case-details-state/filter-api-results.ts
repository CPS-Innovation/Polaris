import { ApiTextSearchResult } from "../../domain/ApiTextSearchResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";

// Due to a limitation of the search service, we may have search results coming through from previous
//  and now stale runs of the pipeline.  As a safety, we filter the incoming search results for document ids that
//  we know we have.
export const filterApiResults = (
  apiResults: ApiTextSearchResult[],
  existingDocuments: MappedCaseDocument[]
) => {
  const knownDocumentIds = existingDocuments.map((item) => item.documentId);

  const searchResultsBelongingToLiveDocuments = apiResults.filter((item) =>
    knownDocumentIds.includes(item.documentId)
  );

  return searchResultsBelongingToLiveDocuments;
};
