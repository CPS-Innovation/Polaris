import { sortRedactionHighlights } from "./sortRedactionHighlights";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";

describe("sortRedactionHighlights", () => {
  it("It should sort the highlight based on page number", () => {
    const highlights = [
      { position: { pageNumber: 1, boundingRect: { x1: 397, y1: 950 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 570, y1: 749 } } },
      { position: { pageNumber: 4, boundingRect: { x1: 192, y1: 143 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 194, y1: 324 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 632, y1: 307 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 103, y1: 442 } } },
    ];
    const expectedResult = [
      { position: { pageNumber: 1, boundingRect: { x1: 632, y1: 307 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 194, y1: 324 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 397, y1: 950 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 103, y1: 442 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 570, y1: 749 } } },
      { position: { pageNumber: 4, boundingRect: { x1: 192, y1: 143 } } },
    ];

    const sortedHighlights = sortRedactionHighlights(
      highlights as IPdfHighlight[]
    );

    expect(sortedHighlights).toStrictEqual(expectedResult);
  });

  it("It should sort the highlight based on position top-left to bottom-right", () => {
    const highlights = [
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 10 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 0, y1: 22 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 35 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 50, y1: 10 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 9, y1: 21 } } },
    ];
    const expectedResult = [
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 10 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 50, y1: 10 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 0, y1: 22 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 9, y1: 21 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 35 } } },
    ];

    const sortedHighlights = sortRedactionHighlights(
      highlights as IPdfHighlight[]
    );

    expect(sortedHighlights).toStrictEqual(expectedResult);
  });
  it("It should sort the highlight based on position top-left to bottom-right with an allowed y-axis variation of <10", () => {
    const highlights = [
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 5 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 8, y1: 30 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 0, y1: 18 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 27 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 8, y1: 30 } } },

      { position: { pageNumber: 2, boundingRect: { x1: 9, y1: 10 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 10, y1: 27 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 10, y1: 5 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 0, y1: 18 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 9, y1: 10 } } },
    ];
    const expectedResult = [
      { position: { pageNumber: 1, boundingRect: { x1: 0, y1: 18 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 9, y1: 10 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 5 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 8, y1: 30 } } },
      { position: { pageNumber: 1, boundingRect: { x1: 10, y1: 27 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 0, y1: 18 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 9, y1: 10 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 10, y1: 5 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 8, y1: 30 } } },
      { position: { pageNumber: 2, boundingRect: { x1: 10, y1: 27 } } },
    ];

    const sortedHighlights = sortRedactionHighlights(
      highlights as IPdfHighlight[]
    );

    expect(sortedHighlights).toStrictEqual(expectedResult);
  });
});
