import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import {
  WitnessIndicator,
  witnessIndicatorLetters,
} from "../../domain/WitnessIndicators";
import { Witness } from "../../domain/gateway/CaseDetails";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
import { getCategory } from "./document-category-definitions";
import { getDocumentAttachments } from "./document-category-helpers";
import {
  readFromLocalStorage,
  ReadData,
} from "../../presentation/case-details/utils/localStorageUtils";

export const mapDocumentsState = (
  result: PresentationDocumentProperties[],
  witnesses: Witness[],
  caseId: number
): AsyncResult<MappedCaseDocument[]> => {
  const docReadData = readFromLocalStorage(caseId, "read") as ReadData | null;
  const data = result.map((item) => {
    const { category, subCategory } = getCategory(item);
    const witnessesForDoc = witnesses.find((w) => w.id === item.witnessId);
    const docRead = docReadData
      ? docReadData[`${item.documentId}`] ?? false
      : false;
    return {
      ...item,
      presentationFileName: item.presentationTitle,
      presentationCategory: category,
      presentationSubCategory: subCategory,
      docRead: docRead,
      attachments: getDocumentAttachments(item, result),
      witnessIndicators: witnessesForDoc
        ? mapWitnessIndicators(witnessesForDoc)
        : [],
    };
  });

  return {
    status: "succeeded",
    data,
  };
};

const mapWitnessIndicators = (witness: Witness): WitnessIndicator[] => {
  const keys = Object.keys(witnessIndicatorLetters) as (keyof Witness)[];

  return keys
    .filter((prop) => witness[prop])
    .map((prop) => witnessIndicatorLetters[prop]) as WitnessIndicator[];
};
