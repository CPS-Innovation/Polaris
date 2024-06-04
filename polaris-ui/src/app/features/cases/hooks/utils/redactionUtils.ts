import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";
import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import {
  RedactionsData,
  readFromLocalStorage,
  deleteFromLocalStorage,
  addToLocalStorage,
} from "../../presentation/case-details/utils/localStorageUtils";
import { round } from "lodash";

/**
 * This is normalize redaction highlights based on the page height of the first redaction on each page
 * @param redactionHighlights
 * @returns
 */
export const getNormalizedRedactionHighlights = (
  redactionHighlights: IPdfHighlight[] | ISearchPIIHighlight[]
) => {
  const basePageHeights = {} as any;
  const normalizedHighlights = redactionHighlights.map((highlight) => {
    const { position } = highlight;
    const { pageNumber } = position;

    const { width, x1, x2, y1, y2, height } = position.boundingRect;
    if (!basePageHeights[`${pageNumber}`]) {
      basePageHeights[`${pageNumber}`] = height;
    }
    const baseHeight = basePageHeights[`${pageNumber}`];
    if (height !== baseHeight) {
      const scaleFactor = (baseHeight - height) / height;
      return {
        ...highlight,
        position: {
          ...position,
          boundingRect: {
            height: baseHeight,
            x1: roundToFixedDecimalPlaces(x1 + x1 * scaleFactor),
            y1: roundToFixedDecimalPlaces(y1 + y1 * scaleFactor),
            x2: roundToFixedDecimalPlaces(x2 + x2 * scaleFactor),
            y2: roundToFixedDecimalPlaces(y2 + y2 * scaleFactor),
            width: roundToFixedDecimalPlaces(width + width * scaleFactor),
          },
          rects: position.rects.map((rect) => ({
            height: baseHeight,
            x1: roundToFixedDecimalPlaces(rect.x1 + rect.x1 * scaleFactor),
            y1: roundToFixedDecimalPlaces(rect.y1 + rect.y1 * scaleFactor),
            x2: roundToFixedDecimalPlaces(rect.x2 + rect.x2 * scaleFactor),
            y2: roundToFixedDecimalPlaces(rect.y2 + rect.y2 * scaleFactor),
            width: roundToFixedDecimalPlaces(
              rect.width + rect.width * scaleFactor
            ),
          })),
        },
      };
    }

    return highlight;
  });
  return normalizedHighlights;
};

export const roundToFixedDecimalPlaces = (
  num: number,
  precisionCount: number = 2
) => round(num, precisionCount);

export const getRedactionsToSaveLocally = (
  items: CaseDocumentViewModel[],
  documentId: string,
  caseId: number
) => {
  const locallySavedRedactions =
    (readFromLocalStorage(caseId, "redactions") as RedactionsData) ?? [];

  const redactionHighlights = items.find(
    (item) => item.documentId === documentId
  )?.redactionHighlights;

  const filteredRedactions = locallySavedRedactions.filter(
    (redaction) => redaction.documentId !== documentId
  );

  if (redactionHighlights?.length) {
    filteredRedactions.push({
      documentId: documentId,
      redactionHighlights: redactionHighlights,
    });
  }

  return filteredRedactions;
};

export const getLocallySavedRedactionHighlights = (
  documentId: string,
  caseId: number
) => {
  const redactionsData = readFromLocalStorage(
    caseId,
    "redactions"
  ) as RedactionsData | null;
  if (!redactionsData) {
    return [];
  }
  return (
    redactionsData.find((data) => data.documentId === documentId)
      ?.redactionHighlights ?? []
  );
};

export const handleRemoveLocallySavedRedactions = (
  documentId: string,
  caseId: number
) => {
  const redactionsData =
    (readFromLocalStorage(caseId, "redactions") as RedactionsData) ?? [];

  const newData = redactionsData.filter(
    (data) => data.documentId !== documentId
  );
  if (newData.length) {
    addToLocalStorage(caseId, "redactions", newData);
    return;
  }
  deleteFromLocalStorage(caseId, "redactions");
};
