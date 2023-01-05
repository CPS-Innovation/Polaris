import { CombinedState } from "../../domain/CombinedState";
import { MappedDocumentResult } from "../../domain/MappedDocumentResult";

export const isDocumentVisible = (
  {
    cmsDocType: { code },
    presentationCategory: docCategory,
    isVisible: existingIsVisible,
  }: MappedDocumentResult,
  { docType, category }: CombinedState["searchState"]["filterOptions"]
) => {
  const isAnyFilterActive =
    Object.values(docType).some((item) => item.isSelected) ||
    Object.values(category).some((item) => item.isSelected);

  const isVisibleForDocType = !!docType[code]?.isSelected;

  const isVisibleForCategory = !!category[docCategory]?.isSelected;

  const isVisible =
    !isAnyFilterActive || isVisibleForDocType || isVisibleForCategory;

  return {
    isVisible,
    hasChanged: isVisible !== existingIsVisible,
  };
};
