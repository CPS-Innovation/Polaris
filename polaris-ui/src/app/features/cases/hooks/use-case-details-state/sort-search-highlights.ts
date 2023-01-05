import { IPdfHighlight } from "../../domain/IPdfHighlight";

// Sort to get a nice top-to-bottom, left-to-right sequence as the user iterates through
//  the search highlights using the prev/next buttons
export const sortSearchHighlights = (
  highlights: IPdfHighlight[]
): IPdfHighlight[] =>
  highlights
    .slice()
    .sort(
      (a: IPdfHighlight, b: IPdfHighlight) =>
        a.position.pageNumber - b.position.pageNumber ||
        a.position.boundingRect.y1 - b.position.boundingRect.y1 ||
        a.position.boundingRect.x1 - b.position.boundingRect.x1
    );
