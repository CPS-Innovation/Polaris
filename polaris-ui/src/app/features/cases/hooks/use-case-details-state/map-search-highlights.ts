import { IPdfHighlight } from "../../domain/IPdfHighlight";

const PADDING_INCHES = 0.03;

export const mapSearchHighlights = (
  pageOccurrences: {
    pageIndex: number;
    pageHeight: number;
    pageWidth: number;
    boundingBoxes: number[][];
  }[]
): IPdfHighlight[] => {
  const results: IPdfHighlight[] = [];

  let i = 0;
  for (const {
    boundingBoxes,
    pageIndex,
    pageHeight,
    pageWidth,
  } of pageOccurrences) {
    for (const [x1, y1, , , x2, y2] of boundingBoxes) {
      const rect = {
        x1: x1 - PADDING_INCHES,
        y1: y1 - PADDING_INCHES,
        x2: x2 + PADDING_INCHES,
        y2: y2 + PADDING_INCHES,
        width: pageWidth,
        height: pageHeight,
      };
      results.push({
        // note: subsequent sorting means that the ids emerging from this function
        //  are not in regular ascending order
        id: String(i++),
        type: "search",
        highlightType: "linear",
        position: {
          pageNumber: pageIndex,
          boundingRect: rect,
          rects: [rect],
        },
        redactionType: "Address",
      });
    }
  }
  return results;
};
