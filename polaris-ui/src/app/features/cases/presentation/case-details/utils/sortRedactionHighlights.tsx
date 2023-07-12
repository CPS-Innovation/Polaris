import { IPdfHighlight } from "../../../domain/IPdfHighlight";
const Y_AXIS_VARIATION = 10;
export const sortRedactionHighlights = (elements: IPdfHighlight[]) => {
  //first sort based on page number and then y position and x position (top-left to bottom-right)
  elements.sort((a: IPdfHighlight, b: IPdfHighlight) => {
    if (a.position.pageNumber > b.position.pageNumber) {
      return 1;
    }
    if (a.position.boundingRect.y1 - b.position.boundingRect.y1 === 0) {
      return a.position.boundingRect.x1 > b.position.boundingRect.x1 ? 1 : -1;
    }
    return a.position.boundingRect.y1 > b.position.boundingRect.y1 ? 1 : -1;
  });

  //trying to add some y-axis variation of 10 units and group them together based on x-axis position (for visual impact)
  return elements.sort((a: IPdfHighlight, b: IPdfHighlight) => {
    if (a.position.pageNumber > b.position.pageNumber) {
      return 1;
    }
    if (
      Math.abs(a.position.boundingRect.y1 - b.position.boundingRect.y1) <
      Y_AXIS_VARIATION
    ) {
      return a.position.boundingRect.x1 > b.position.boundingRect.x1 ? 1 : -1;
    }
    return 0;
  });
};
