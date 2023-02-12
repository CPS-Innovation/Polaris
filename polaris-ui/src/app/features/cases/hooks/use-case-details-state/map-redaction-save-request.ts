import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { RedactionSavePage } from "../../domain/RedactionSavePage";
import { RedactionSaveRequest } from "../../domain/RedactionSaveRequest";

export const mapRedactionSaveRequest = (
  pdfId: RedactionSaveRequest["docId"],
  redactionHighlights: IPdfHighlight[]
) => {
  const redactionSaveRequest = redactionHighlights.reduce(
    (acc, curr) => {
      const { position, highlightType } = curr;

      let redactionPage = acc.redactions.find(
        (item) => item.pageIndex === position.pageNumber
      );

      const { height, width } = position.boundingRect;
      if (!redactionPage) {
        const { pageNumber: pageIndex } = position;

        redactionPage = {
          pageIndex: pageIndex,
          height,
          width,
          redactionCoordinates: [],
        };
        acc.redactions.push(redactionPage);
      }

      const rects =
        highlightType === "area" ? [position.boundingRect] : position.rects;

      redactionPage.redactionCoordinates.push(
        ...rects.map((item) => ({
          x1: item.x1,
          y1: height - item.y1,
          x2: item.x2,
          y2: height - item.y2,
        }))
      );

      return acc;
    },
    {
      docId: pdfId,
      redactions: [] as RedactionSavePage[],
    } as RedactionSaveRequest
  );

  return redactionSaveRequest;
};
