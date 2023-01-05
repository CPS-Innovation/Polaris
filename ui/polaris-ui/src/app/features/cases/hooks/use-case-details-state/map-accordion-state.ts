import { ApiResult } from "../../../../common/types/ApiResult";
import { AsyncResult } from "../../../../common/types/AsyncResult";

import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { AccordionDocumentSection } from "../../presentation/case-details/accordion/types";
import { categoryNamesInPresentationOrder } from "./document-category-definitions";

export const mapAccordionState = (
  documentsState: ApiResult<MappedCaseDocument[]>
): AsyncResult<AccordionDocumentSection[]> => {
  if (documentsState.status !== "succeeded") {
    // we wait for documentsState to be ready, even if pipeline results arrive first
    return {
      status: "loading",
    };
  }

  // first make sure we have every category section represented in our results
  //  (we want to return sections even if they are empty)
  const results: AccordionDocumentSection[] =
    categoryNamesInPresentationOrder.map((category) => ({
      sectionId: category,
      sectionLabel: category,
      docs: [],
    }));

  // cycle through each doc from the api
  for (const caseDocument of documentsState.data) {
    const resultItem = results.find(
      (item) => item.sectionId === caseDocument.presentationCategory
    )!;

    // ... add to the section results ...
    resultItem.docs.push({
      ...caseDocument,
    });
  }

  return { status: "succeeded", data: results };
};
