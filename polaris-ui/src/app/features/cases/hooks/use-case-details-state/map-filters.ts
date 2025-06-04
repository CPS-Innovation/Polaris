import { CombinedState } from "../../domain/CombinedState";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";
import { categoryNamesInPresentationOrder } from "./document-category-definitions";

export const mapFilters = (
  mappedTextSearchResult: MappedTextSearchResult
): CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"] => {
  // we show categories even if they are empty, so kick off with map of all the
  //  categories
  const category = categoryNamesInPresentationOrder.reduce(
    (accumulator, categoryName) => {
      accumulator[categoryName] = {
        label: categoryName,
        count: 0,
        isSelected: false,
      };
      return accumulator;
    },
    {} as CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"]["category"]
  );

  // Sort documents by their documentType using optional chaining and a named compare function.
  function compareByDocumentType(
    a: typeof mappedTextSearchResult.documentResults[number],
    b: typeof mappedTextSearchResult.documentResults[number]
  ): number {
    const docTypeA = a.cmsDocType?.documentType ?? "";
    const docTypeB = b.cmsDocType?.documentType ?? "";
    if (docTypeA < docTypeB) return -1;
    if (docTypeA > docTypeB) return 1;
    return 0;
  }

  const orderedDocumentResults = mappedTextSearchResult.documentResults.slice().sort(compareByDocumentType);

  const docType =
    {} as CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"]["docType"];

  for (const doc of orderedDocumentResults) {
    if (!docType[doc.cmsDocType.documentType]) {
      docType[doc.cmsDocType.documentType] = {
        label: doc.cmsDocType.documentType || "Unknown",
        count: 0,
        isSelected: false,
      };
    }

    docType[doc.cmsDocType.documentType].count += 1;
    category[doc.presentationCategory].count += 1;
  }

  return {
    category,
    docType,
  };
};
