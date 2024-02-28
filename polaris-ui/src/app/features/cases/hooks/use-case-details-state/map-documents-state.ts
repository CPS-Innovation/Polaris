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

export const mapDocumentsState = (
  result: PresentationDocumentProperties[],
  witnesses: Witness[],
  caseId: number
): AsyncResult<MappedCaseDocument[]> => {
  const data = result.map((item) => {
    const { category, subCategory } = getCategory(item);
    const witnessesForDoc = witnesses.find((w) => w.id === item.witnessId);
    return {
      ...item,
      presentationFileName: item.presentationTitle,
      presentationCategory: category,
      presentationSubCategory: subCategory,
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
