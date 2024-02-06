import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { round } from "lodash";

export const getNormalizedRedactionHighlights = (
  redactionHighlights: IPdfHighlight[],
  baseHeight: number
) => {
  const normalizedHighlights = redactionHighlights.map((highlight) => {
    const { position } = highlight;

    const { width, x1, x2, y1, y2, height } = position.boundingRect;
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
