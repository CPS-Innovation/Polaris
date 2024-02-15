import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { RedactionSavePage } from "../../domain/gateway/RedactionSavePage";
import { RedactionSaveRequest } from "../../domain/gateway/RedactionSaveRequest";
import {
  getNormalizedRedactionHighlights,
  roundToFixedDecimalPlaces,
} from "../utils/redactionUtils";

export const mapRedactionSaveRequest = (
  documentId: RedactionSaveRequest["documentId"],
  redactionHighlights: IPdfHighlight[]
) => {
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

  return {
    documentId,
    redactions,
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
