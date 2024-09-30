import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";
import { IPageDeleteRedaction } from "../../domain/IPageDeleteRedaction";
import { RedactionSavePage } from "../../domain/gateway/RedactionSavePage";
import {
  RedactionSaveData,
  PIIAnalyticsData,
} from "../../domain/gateway/RedactionSaveRequest";
import {
  getNormalizedRedactionHighlights,
  roundToFixedDecimalPlaces,
} from "../utils/redactionUtils";

export const mapRedactionSaveRequest = (
  documentId: string,
  redactionHighlights: IPdfHighlight[] | ISearchPIIHighlight[],
  pageDeleteRedactions: IPageDeleteRedaction[]
): RedactionSaveData => {
  const redactions = [] as RedactionSavePage[];

  const normalizedHighlights =
    getNormalizedRedactionHighlights(redactionHighlights);

  for (const redactionHighlight of normalizedHighlights) {
    const { position, highlightType } = redactionHighlight;
    const { pageNumber: pageIndex } = position;
    const { height, width } = position.boundingRect;

    const redactionPageExists = redactions.some(
      (item) => item.pageIndex === position.pageNumber
    );

    if (!redactionPageExists) {
      redactions.push({
        pageIndex,
        height: roundToFixedDecimalPlaces(height),
        width: roundToFixedDecimalPlaces(width),
        redactionCoordinates: [],
      });
    }

    const redactionPage = redactions.find(
      (item) => item.pageIndex === position.pageNumber
    )!;

    const rects =
      highlightType === "area" ? [position.boundingRect] : position.rects;

    redactionPage.redactionCoordinates.push(
      ...rects.map((item) => ({
        x1: item.x1 > 0 ? roundToFixedDecimalPlaces(item.x1) : 0,
        y1: roundToFixedDecimalPlaces(getSafeCoordinate(height, item.y1)),
        x2: roundToFixedDecimalPlaces(item.x2),
        y2: roundToFixedDecimalPlaces(getSafeCoordinate(height, item.y2)),
      }))
    );
  }

  const pageDeletes = pageDeleteRedactions.map((redaction) => ({
    pageIndex: redaction.pageNumber,
    operation: "delete" as const,
  }));

  return {
    documentId,
    redactions,
    documentModifications: pageDeletes,
  };
};

const getSafeCoordinate = (height: number, y: number) => {
  // It is possible for the user to draw an "area" redaction that begins at
  //  the foot of a page and then continues on to the next page. The UI will
  //  only indicate the part of the redaction that is on the start page.
  //  However the coordinates of the end point of the redaction are returned,
  //  meaning ultimately we pass invalid coordinates to the back end, which do not
  //  match the visible redaction as presented to the user. So we correct here.
  if (y > height) {
    return 0;
  }
  // It is possible for the user to draw an "area" redaction that begins at
  //  the top of a page and then upwards continues on to the preceding page. The UI will
  //  only indicate the part of the redaction that is on the start page.
  //  However the coordinates of the end point of the redaction are returned,
  //  meaning ultimately we pass invalid coordinates to the back end, which do not
  //  match the visible redaction as presented to the user. So we correct here.
  if (y < 0) {
    return height;
  }
  // The coordinate system of the UI has (0,0) at the top-left corner of a page.  The
  //  back end has (0,0) at the bottom-left, so we transpose here.
  return height - y;
};

export const mapSearchPIISaveRedactionObject = (
  manualRedactionHighlights: IPdfHighlight[],
  searchPIIHighlights: ISearchPIIHighlight[]
): PIIAnalyticsData => {
  if (!searchPIIHighlights.length) {
    return {};
  }
  const amendedRedactions = getAmendedRedactionsCount(
    manualRedactionHighlights,
    searchPIIHighlights
  );

  const piiCategoryGroupedHighlights: Record<string, ISearchPIIHighlight[]> =
    searchPIIHighlights.reduce((acc, highlight) => {
      if (!acc[highlight.piiCategory]) {
        acc[highlight.piiCategory] = [highlight];
        return acc;
      }
      acc[highlight.piiCategory] = [...acc[highlight.piiCategory], highlight];
      return acc;
    }, {} as any);

  const piiData = Object.entries(piiCategoryGroupedHighlights).map(
    ([key, value]) => {
      return {
        polarisCategory: value[0].redactionType.name,
        providerCategory: key,
        countSuggestions: value.length,
        countAccepted: value.filter(
          (val) =>
            val.redactionStatus === "accepted" ||
            val.redactionStatus === "acceptedAll"
        ).length,
        countAmended: amendedRedactions[key] ? amendedRedactions[key] : 0,
      };
    }
  );

  return {
    categories: piiData,
  };
};

const checkIfHighlightIsWithinBox = (
  boundary: {
    height: any;
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    width: number;
  },
  highlight: {
    height: any;
    x1: number;
    y1: number;
    x2: number;
    y2: number;
    width: number;
  }
) => {
  const CORRECTION_VALUE = 10;
  return (
    boundary.x1 - CORRECTION_VALUE <= highlight.x1 &&
    boundary.y1 - CORRECTION_VALUE <= highlight.y1 &&
    boundary.x2 + CORRECTION_VALUE >= highlight.x2 &&
    boundary.y2 + CORRECTION_VALUE >= highlight.y2
  );
};

const getAmendedRedactionsCount = (
  manualRedactionHighlights: IPdfHighlight[],
  searchPIIHighlights: ISearchPIIHighlight[]
): Record<string, number> => {
  if (!manualRedactionHighlights.length || !searchPIIHighlights.length) {
    return {};
  }

  const ignoredHighlights = searchPIIHighlights.filter(
    (highlight) =>
      highlight.redactionStatus === "ignored" ||
      highlight.redactionStatus === "ignoredAll"
  );
  const normalizedHighlights = getNormalizedRedactionHighlights([
    ...manualRedactionHighlights,
    ...ignoredHighlights,
  ]);

  const normalizedSearchPIIHighlights = normalizedHighlights.filter(
    (highlight) => highlight.type === "searchPII"
  ) as ISearchPIIHighlight[];
  const normalizedManualHighlights = normalizedHighlights.filter(
    (highlight) => highlight.type === "redaction"
  );

  let partialRedactions: ISearchPIIHighlight[] = [];

  normalizedManualHighlights.forEach((highlight) => {
    const pageNumber = highlight.position.pageNumber;
    const piiHighlights = normalizedSearchPIIHighlights.filter(
      (highlight) => highlight.position.pageNumber === pageNumber
    );
    const match = piiHighlights.find((item) => {
      return checkIfHighlightIsWithinBox(
        item.position.boundingRect,
        highlight.position.boundingRect
      );
    });
    if (match) partialRedactions.push(match);
  });

  const partialRedactionsSummary = partialRedactions.reduce(
    (acc, highlight) => {
      if (!acc[highlight.piiCategory]) {
        acc[highlight.piiCategory] = 1;
        return acc;
      }
      acc[highlight.piiCategory] = acc[highlight.piiCategory] + 1;
      return acc;
    },
    {} as Record<string, number>
  );

  return partialRedactionsSummary;
};
