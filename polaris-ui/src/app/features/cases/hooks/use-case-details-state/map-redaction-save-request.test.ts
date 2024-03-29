import { mapRedactionSaveRequest } from "./map-redaction-save-request";
import { IPdfHighlight } from "../../domain/IPdfHighlight";

describe("map-redaction-save-request", () => {
  it("maps a redaction highlights to a RedactionSaveRequestObject and transposes y coordinates", () => {
    const result = mapRedactionSaveRequest("1", [
      {
        id: "1",
        type: "redaction",
        highlightType: "area",

        position: {
          pageNumber: 2,
          boundingRect: { x1: 1, y1: 2, x2: 3, y2: 4, height: 100, width: 100 },
        },
      } as IPdfHighlight,
      {
        id: "2",
        type: "redaction",
        highlightType: "area",

        position: {
          pageNumber: 2,

          boundingRect: {
            x1: 11,
            y1: 12,
            x2: 13,
            y2: 14,
            height: 100,
            width: 100,
          },
        },
      } as IPdfHighlight,
      {
        id: "3",
        type: "redaction",
        highlightType: "linear",

        position: {
          boundingRect: {
            height: 100,
            width: 100,
          },
          pageNumber: 3,
          rects: [
            {
              x1: 21,
              y1: 22,
              x2: 23,
              y2: 24,
              height: 100,
              width: 100,
            },
            {
              x1: 31,
              y1: 32,
              x2: 33,
              y2: 34,
              height: 100,
              width: 100,
            },
          ],
        },
      } as IPdfHighlight,
    ]);

    expect(result).toEqual({
      documentId: "1",
      redactions: [
        {
          pageIndex: 2,
          height: 100,
          width: 100,
          redactionCoordinates: [
            { x1: 1, y1: 98, x2: 3, y2: 96 },
            { x1: 11, y1: 88, x2: 13, y2: 86 },
          ],
        },
        {
          pageIndex: 3,
          height: 100,
          width: 100,
          redactionCoordinates: [
            { x1: 21, y1: 78, x2: 23, y2: 76 },
            { x1: 31, y1: 68, x2: 33, y2: 66 },
          ],
        },
      ],
    });
  });

  it("rounds decimal places", () => {
    const result = mapRedactionSaveRequest("1", [
      {
        id: "1",
        type: "redaction",
        highlightType: "area",

        position: {
          pageNumber: 2,
          boundingRect: {
            x1: 1,
            y1: 2,
            x2: 3,
            y2: 4,
            height: 100.123,
            width: 100.456,
          },
        },
      } as IPdfHighlight,
    ]);

    expect(result).toEqual({
      documentId: "1",
      redactions: [
        {
          pageIndex: 2,
          height: 100.12,
          width: 100.46,
          redactionCoordinates: [{ x1: 1, y1: 98.12, x2: 3, y2: 96.12 }],
        },
      ],
    });
  });

  it("converts negative y values to 0 before transposing", () => {
    const result = mapRedactionSaveRequest("1", [
      {
        id: "1",
        type: "redaction",
        highlightType: "area",

        position: {
          pageNumber: 2,
          boundingRect: {
            x1: 1,
            y1: -2,
            x2: 3,
            y2: 4,
            height: 100.123,
            width: 100.456,
          },
        },
      } as IPdfHighlight,
    ]);

    expect(result).toEqual({
      documentId: "1",
      redactions: [
        {
          pageIndex: 2,
          height: 100.12,
          width: 100.46,
          redactionCoordinates: [{ x1: 1, y1: 100.12, x2: 3, y2: 96.12 }],
        },
      ],
    });
  });

  it("converts y values larger than page height to page height before transposing", () => {
    const result = mapRedactionSaveRequest("1", [
      {
        id: "1",
        type: "redaction",
        highlightType: "area",

        position: {
          pageNumber: 2,
          boundingRect: {
            x1: 1,
            y1: 101,
            x2: 3,
            y2: 4,
            height: 100.123,
            width: 100.456,
          },
        },
      } as IPdfHighlight,
    ]);

    expect(result).toEqual({
      documentId: "1",
      redactions: [
        {
          pageIndex: 2,
          height: 100.12,
          width: 100.46,
          redactionCoordinates: [{ x1: 1, y1: 0, x2: 3, y2: 96.12 }],
        },
      ],
    });
  });
});
