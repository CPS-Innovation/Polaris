import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";
import { SearchPIIResultItem } from "../../domain/gateway/SearchPIIData";
import { RedactionTypeData } from "../../domain/redactionLog/RedactionLogData";

const PADDING_INCHES = 0.03;

const getMissedRedactionType = (
  missedRedactionTypes: RedactionTypeData[],
  piiCategory: string
) => {
  return (
    missedRedactionTypes.find(
      (redactionType) =>
        redactionType.name?.toLowerCase() === piiCategory?.toLowerCase()
    ) ?? { id: "", name: "" }
  );
};

export const mapSearchPIIHighlights = (
  searchPIIDataItems: SearchPIIResultItem[],
  missedRedactionTypes: RedactionTypeData[]
): ISearchPIIHighlight[] => {
  let results: ISearchPIIHighlight[] = [];

  const groupWords = searchPIIDataItems.reduce((acc, searchPIIDataItem) => {
    const { words, lineIndex } = searchPIIDataItem;

    words.forEach((item1) => {
      if (item1.piiGroupId) {
        acc[`${item1.piiGroupId}`] = {
          ...acc[`${item1.piiGroupId}`],
          [`${lineIndex}`]: {
            ...(acc[`${item1.piiGroupId}`]?.[`${lineIndex}`] ?? {}),
            ...searchPIIDataItem,
            words: [
              ...(acc[`${item1.piiGroupId}`]?.[`${lineIndex}`]?.words ?? []),
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
    Object.keys(groupWords[groupKey]).forEach((key) => {
      const searchPIIDataItem = groupWords[groupKey][key];
      const { pageIndex, pageHeight, pageWidth, words } = searchPIIDataItem;

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

        const filteredHighlights = results.filter(
          (item) => item.id !== groupKey
        );

        const existingGroupHighlight = results.find(
          (item) => item.id === groupKey
        );

        if (existingGroupHighlight) {
          results = [
            ...filteredHighlights,
            {
              ...existingGroupHighlight,
              textContent: `${existingGroupHighlight.textContent} ${textContent}`,
              position: {
                ...existingGroupHighlight.position,
                rects: [...existingGroupHighlight.position.rects, rect],
              },
            },
          ];
        } else {
          results = [
            ...results,
            {
              // note: subsequent sorting means that the ids emerging from this function
              //  are not in regular ascending order
              id: groupKey,
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
              redactionType: getMissedRedactionType(
                missedRedactionTypes,
                words[0].piiCategory
              ),
              groupId: words[0].piiGroupId,
            },
          ];
        }
      }
    });
  });
  console.log("results>>>", results);
  return results;
};