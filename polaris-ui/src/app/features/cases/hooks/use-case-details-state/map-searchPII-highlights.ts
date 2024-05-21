import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";
import { SearchPIIResultItem } from "../../domain/gateway/SearchPIIData";

const PADDING_INCHES = 0.03;

export const mapSearchPIIHighlights = (
  searchPIIDataItems: SearchPIIResultItem[]
): ISearchPIIHighlight[] => {
  const results: ISearchPIIHighlight[] = [];

  const groupWords = searchPIIDataItems.reduce((acc, searchPIIDataItem) => {
    const { pageIndex, pageHeight, pageWidth, words, id, lineIndex } =
      searchPIIDataItem;

    words.forEach((item1) => {
      if (item1.groupId) {
        acc[`${item1.groupId}`] = {
          ...acc[`${item1.groupId}`],
          [`${lineIndex}`]: {
            ...(acc[`${item1.groupId}`]?.[`${lineIndex}`] ?? {}),
            ...searchPIIDataItem,
            words: [
              ...(acc[`${item1.groupId}`]?.[`${lineIndex}`]?.words ?? []),
              item1,
            ],
          },
        };
      }
    });
    return acc;
  }, {} as Record<string, { [key: string]: SearchPIIResultItem }>);

  console.log("groupWords>>", groupWords);

  Object.keys(groupWords).forEach((groupKey) => {
    console.log(
      "Object.keys(groupWords[groupKey])>>",
      Object.keys(groupWords[groupKey])
    );
    Object.keys(groupWords[groupKey]).forEach((key) => {
      const searchPIIDataItem = groupWords[groupKey][key];
      const { pageIndex, pageHeight, pageWidth, words, id } = searchPIIDataItem;

      if (words[0].boundingBox && words[words.length - 1].boundingBox) {
        const [x1, y1] = words[0].boundingBox;
        const [, , , , x2, y2] = words[words.length - 1].boundingBox!;

        const rect = {
          x1: x1 - PADDING_INCHES,
          y1: y1 - PADDING_INCHES,
          x2: x2 + PADDING_INCHES,
          y2: y2 + PADDING_INCHES,
          width: pageWidth,
          height: pageHeight,
        };

        const textContent = words.reduce((acc, word) => {
          acc = acc ? `${acc} ${word.text}` : word.text;
          return acc;
        }, "");

        results.push({
          // note: subsequent sorting means that the ids emerging from this function
          //  are not in regular ascending order
          id: `${groupKey}-${key}`,
          type: "searchPII",
          highlightType: "linear",
          redactionStatus: "redacted",
          textContent: textContent,
          position: {
            pageNumber: pageIndex,
            boundingRect: rect,
            rects: [rect],
          },
          piiCategory: words[0].piiCategory,
        });
      }
    });
  });
  console.log("results>>>", results);
  return results;
};
