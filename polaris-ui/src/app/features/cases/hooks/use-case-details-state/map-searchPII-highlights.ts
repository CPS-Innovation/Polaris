import { Scaled } from "../../../../../react-pdf-highlighter";
import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";
import { SearchPIIResultItem } from "../../domain/gateway/SearchPIIData";
import { RedactionTypeData } from "../../domain/redactionLog/RedactionLogData";

const PADDING_INCHES = 0.03;

const getMissedRedactionType = (
  missedRedactionTypes: RedactionTypeData[],
  redactionType: string
) => {
  return (
    missedRedactionTypes.find(
      (type) => type.name?.toLowerCase() === redactionType?.toLowerCase()
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
          acc = acc ? `${acc} ${word.sanitizedText}` : word.sanitizedText;
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
              id: groupKey,
              type: "searchPII",
              highlightType: "linear",
              redactionStatus: "initial",
              textContent: textContent,
              position: {
                pageNumber: pageIndex,
                boundingRect: rect,
                rects: [rect],
              },
              piiCategory: words[0].piiCategory,
              redactionType: getMissedRedactionType(
                missedRedactionTypes,
                words[0]?.redactionType
              ),
              groupId: words[0].piiGroupId,
            },
          ];
        }
      }
    });
  });

  const filteredResult = getFilteredByValidUkPhoneNumber(results);

  const highlightsWithMultiLineBoundingBox = filteredResult.map((result) => {
    if (result.position.rects.length > 1) {
      return {
        ...result,
        position: {
          ...result.position,
          boundingRect: getBoundingBoxFromRects(result.position.rects),
        },
      };
    }
    return result;
  });

  return highlightsWithMultiLineBoundingBox;
};

const getBoundingBoxFromRects = (rects: Scaled[]) => {
  const boundingRect = rects.reduce((acc, value) => {
    if (!acc.height) {
      acc = { ...value };
      return acc;
    }
    if (value.x1 < acc.x1) {
      acc.x1 = value.x1;
    }
    if (value.y1 < acc.y1) {
      acc.y1 = value.y1;
    }
    if (value.x2 > acc.x2) {
      acc.x2 = value.x2;
    }
    if (value.y2 > acc.y2) {
      acc.y2 = value.y2;
    }
    return acc;
  }, {} as Scaled);

  return boundingRect;
};

export const getFilteredByValidUkPhoneNumber = (
  results: ISearchPIIHighlight[]
) => {
  const UK_PHONE_NUMBER_REGEX =
    /^(\+447\d{8,9}|\+44([1-3]|[8-9])\d{8,9}|07\d{8,9}|0([1-3]|[8-9])\d{8,9}|\(?0([1-3]|[8-9])\d{2}\)?[-]?\d{3}[-]?\d{3,4}|\(?(0([1-3]|[8-9]))\d{3}\)?[-]?\d{3}[-]?\d{2,3})$/gm;
  return results.filter((highlight) => {
    if (highlight.piiCategory === "PhoneNumber") {
      const trimmedNumber = highlight.textContent.replace(/\s+/g, "");
      return !!trimmedNumber.match(UK_PHONE_NUMBER_REGEX);
    }
    return true;
  });
};
