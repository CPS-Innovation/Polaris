import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { getFileNameWithoutExtension } from "../../logic/get-file-name-without-extension";
import { getCategory } from "./document-category-definitions";

export const mapDocumentsState = (
  result: PresentationDocumentProperties[]
): AsyncResult<MappedCaseDocument[]> => {
  const data = result.map((item) => ({
    ...item,
    presentationFileName: getFileNameWithoutExtension(item.cmsOriginalFileName),
    presentationCategory: getCategory(item),
  }));

  return {
    status: "succeeded",
    data,
  };
};
