import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";
import { SearchPIIResultItem } from "../../domain/gateway/SearchPIIData";

const PADDING_INCHES = 0.03;

export const mapSearchPIIHighlights = (
  searchPIIDataItems: SearchPIIResultItem[]
): ISearchPIIHighlight[] => {
  const results: ISearchPIIHighlight[] = [];

  searchPIIDataItems.forEach((searchPIIDataItem) => {
    const { pageIndex, pageHeight, pageWidth, words, id } = searchPIIDataItem;
    // let i = 0;
    words
      .filter((word) => word.boundingBox !== null)
      .map((word, i) => {
        if (word.boundingBox) {
          const [x1, y1, , , x2, y2] = word.boundingBox;
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
            id: `${id}-${i}`,
            type: "searchPII",
            highlightType: "linear",
            redactionStatus: "redacted",
            textContent: word.text,
            position: {
              pageNumber: pageIndex,
              boundingRect: rect,
              rects: [rect],
            },
            piiCategory: word.piiCategory,
          });
        }
      });
  });
  return results;
};
