import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PresentationDocumentProperties } from "../../domain/PipelineDocument";
import { getFileNameWithoutExtension } from "../../logic/get-file-name-without-extension";
import { getCategory } from "./document-category-definitions";

export const mapDocumentsState = (
  result: PresentationDocumentProperties[]
): AsyncResult<MappedCaseDocument[]> => ({
  status: "succeeded",
  data: result.map((item, index) => ({
    ...item,
    tabSafeId: `d${index}`,
    presentationFileName: getFileNameWithoutExtension(item.cmsOriginalFileName),
    presentationCategory: getCategory(item),
  })),
});
