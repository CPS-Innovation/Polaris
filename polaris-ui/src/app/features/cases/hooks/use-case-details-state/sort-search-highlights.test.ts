import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { sortSearchHighlights } from "./sort-search-highlights";

describe("sortSearchHighlights", () => {
  it("can sort highlights based on page number ascending", () => {
    const highlightA = {
      position: { pageNumber: 1, boundingRect: { x1: 3, y1: 3 } },
    } as IPdfHighlight;

    const highlightB = {
      position: { pageNumber: 2, boundingRect: { x1: 2, y1: 2 } },
    } as IPdfHighlight;

    const highlightC = {
      position: { pageNumber: 3, boundingRect: { x1: 1, y1: 1 } },
    } as IPdfHighlight;

    const highlightD = {
      position: { pageNumber: 4, boundingRect: { x1: 1, y1: 1 } },
    } as IPdfHighlight;

    const highlightE = {
      position: { pageNumber: 5, boundingRect: { x1: 1, y1: 1 } },
    } as IPdfHighlight;

    expect(
      sortSearchHighlights([
        highlightD,
        highlightE,
        highlightC,
        highlightB,
        highlightA,
      ])
    ).toEqual([highlightA, highlightB, highlightC, highlightD, highlightE]);
  });

  it("can sort highlights based on page number ascending, then by y1", () => {
    const highlightA = {
      position: { pageNumber: 1, boundingRect: { x1: 3, y1: 1 } },
    } as IPdfHighlight;

    const highlightB = {
      position: { pageNumber: 1, boundingRect: { x1: 2, y1: 2 } },
    } as IPdfHighlight;

    const highlightC = {
      position: { pageNumber: 1, boundingRect: { x1: 1, y1: 3 } },
    } as IPdfHighlight;

    const highlightD = {
      position: { pageNumber: 2, boundingRect: { x1: 1, y1: 1 } },
    } as IPdfHighlight;

    const highlightE = {
      position: { pageNumber: 2, boundingRect: { x1: 1, y1: 3 } },
    } as IPdfHighlight;

    expect(
      sortSearchHighlights([
        highlightE,
        highlightC,
        highlightD,
        highlightB,
        highlightA,
      ])
    ).toEqual([highlightA, highlightB, highlightC, highlightD, highlightE]);
  });

  it("can sort highlights based on page number ascending, then by y1, then by x1", () => {
    const highlightA = {
      position: { pageNumber: 1, boundingRect: { x1: 1, y1: 1 } },
    } as IPdfHighlight;

    const highlightB = {
      position: { pageNumber: 1, boundingRect: { x1: 2, y1: 1 } },
    } as IPdfHighlight;

    const highlightC = {
      position: { pageNumber: 1, boundingRect: { x1: 3, y1: 1 } },
    } as IPdfHighlight;

    const highlightD = {
      position: { pageNumber: 2, boundingRect: { x1: 1, y1: 1 } },
    } as IPdfHighlight;

    const highlightE = {
      position: { pageNumber: 2, boundingRect: { x1: 2, y1: 1 } },
    } as IPdfHighlight;

    expect(
      sortSearchHighlights([
        highlightE,
        highlightC,
        highlightD,
        highlightB,
        highlightA,
      ])
    ).toEqual([highlightA, highlightB, highlightC, highlightD, highlightE]);
  });
});
