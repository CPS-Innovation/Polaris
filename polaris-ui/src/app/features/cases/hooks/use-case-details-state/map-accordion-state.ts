import { ApiResult } from "../../../../common/types/ApiResult";
import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { AccordionDocumentSection } from "../../presentation/case-details/accordion/types";
import {
  categoryNamesInPresentationOrder,
  getCategorySort,
} from "./document-category-definitions";

export const mapAccordionState = (
  documentsState: ApiResult<MappedCaseDocument[]>
): AsyncResult<AccordionDocumentSection[]> => {
  if (documentsState.status !== "succeeded") {
    // We wait for documentsState to be ready, even if pipeline results arrive first
    return {
      status: "loading",
    };
  }

  const nonDACDocuments = documentsState.data.filter(
    (document) => document.cmsDocType.documentType !== "DAC"
  );

  // Make sure we have every category section represented in our results
  //  (we want to return sections even if they are empty)
  const data = categoryNamesInPresentationOrder
    .map((category) => ({
      sectionId: category,
      sectionLabel: category,
    }))
    .map((section) => ({
      ...section,
      docs: getCategorySort(section)(
        nonDACDocuments
          .filter((doc) => doc.presentationCategory === section.sectionId)
          .map((doc) => ({ ...doc }))
      ),
    })) as AccordionDocumentSection[];

  return { status: "succeeded", data };
};
