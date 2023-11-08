import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { getCategory } from "./document-category-definitions";

export const mapDocumentsState = (
  result: PresentationDocumentProperties[]
): AsyncResult<MappedCaseDocument[]> => {
  const data = result.map((item) => {
    const { category, subCategory } = getCategory(item);
    return {
      ...item,
      presentationFileName: item.presentationTitle,
      presentationCategory: category,
      presentationSubCategory: subCategory,
    };
  });

  return {
    status: "succeeded",
    data,
  };
};
