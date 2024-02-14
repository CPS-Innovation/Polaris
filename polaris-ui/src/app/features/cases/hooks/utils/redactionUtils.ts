import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { RedactionSaveRequest } from "../../../cases/domain/gateway/RedactionSaveRequest";
import { round } from "lodash";

export const getNormalizedRedactionHighlights = (
  redactionHighlights: IPdfHighlight[]
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

export const getNormalizedRedactionRequest = (
  expectedRequest: RedactionSaveRequest,
  redactionRequest: RedactionSaveRequest
) => {
  const normalizedRedactions = redactionRequest.redactions.map(
    (redaction, index) => {
      const { height, width, redactionCoordinates } = redaction;
      const baseHeight = expectedRequest.redactions[index].height;

      if (height !== baseHeight) {
        const scaleFactor = (baseHeight - height) / height;
        return {
          ...redaction,
          height: baseHeight,
          width: width + width * scaleFactor,
          redactionCoordinates: redactionCoordinates.map((coordinate) => ({
            x1: coordinate.x1 + coordinate.x1 * scaleFactor,
            y1: coordinate.y1 + coordinate.y1 * scaleFactor,
            x2: coordinate.x2 + coordinate.x2 * scaleFactor,
            y2: coordinate.y2 + coordinate.y2 * scaleFactor,
          })),
        };
      }
      return redaction;
    }
  );
  return {
    documentId: redactionRequest.documentId,
    redactions: normalizedRedactions,
  };
};
