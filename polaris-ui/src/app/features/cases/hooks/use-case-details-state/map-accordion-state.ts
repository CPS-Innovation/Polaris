import { ApiResult } from "../../../../common/types/ApiResult";
import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  AccordionDocumentSection,
  AccordionData,
} from "../../presentation/case-details/accordion/types";
import {
  categoryNamesInPresentationOrder,
  getCategorySort,
} from "./document-category-definitions";
import { buildAccordionSectionOpenInitialState } from "../utils/accordionUtils";

export const mapAccordionState = (
  documentsState: ApiResult<MappedCaseDocument[]>,
  accordionState: AsyncResult<AccordionData>
): AsyncResult<AccordionData> => {
  if (documentsState.status !== "succeeded") {
    // We wait for documentsState to be ready, even if pipeline results arrive first
    return {
      status: "loading",
    };
  }

  const currentStateData =
    accordionState.status === "succeeded" ? accordionState.data : null;

  const nonDACDocuments = documentsState.data.filter(
    (document) => document.cmsDocType.documentType !== "DAC"
  );

  // Make sure we have every category section represented in our results
  //  (we want to return sections even if they are empty)
  const sections = categoryNamesInPresentationOrder
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
      ).map((doc) => ({ documentId: doc.documentId })),
    })) as AccordionDocumentSection[];

  const initialState = buildAccordionSectionOpenInitialState(
    sections.map((section) => section.sectionLabel)
  );

  const { sectionsOpenStatus, isAllOpen } = currentStateData ?? initialState;

  return {
    status: "succeeded",
    data: {
      sections,
      sectionsOpenStatus: sectionsOpenStatus,
      isAllOpen: isAllOpen,
    },
  };
};
