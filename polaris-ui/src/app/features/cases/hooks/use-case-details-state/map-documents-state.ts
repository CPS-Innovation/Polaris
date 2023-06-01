import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { getCategory } from "./document-category-definitions";

export const mapDocumentsState = (
  result: PresentationDocumentProperties[]
): AsyncResult<MappedCaseDocument[]> => ({
  status: "succeeded",
  data: result.map((item) => ({
    ...item,
    presentationFileName: item.presentationTitle,
    presentationCategory: getCategory(item),
  })),
});
