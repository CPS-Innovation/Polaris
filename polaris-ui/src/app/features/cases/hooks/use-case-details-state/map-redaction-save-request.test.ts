import {
  mapRedactionSaveRequest,
  mapSearchPIISaveRedactionObject,
} from "./map-redaction-save-request";
import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";

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

describe("mapSearchPIISaveRedactionObject", () => {
  it("Should correctly calculate the pii data and a single manual highlight should only be counted once across only pii categories when calculating countAmended", () => {
    const manualHighlight = [
      {
        highlightType: "linear",
        id: "1717577678814-0",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 304.74713134765625,
            x2: 344.48260498046875,
            y1: 358.4652099609375,
            y2: 376.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 304.74713134765625,
              x2: 344.48260498046875,
              y1: 358.4652099609375,
              y2: 376.9652099609375,
            },
          ],
        },
        redactionType: { id: "1", name: "Named individual" },
        textContent: ": Com",
        type: "redaction",
      },
    ] as IPdfHighlight[];

    const suggestedHighlights = [
      {
        groupId: "1",
        piiCategory: "Person",
        redactionStatus: "ignored",
        highlightType: "linear",
        id: "1",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 304.74713134765625,
            x2: 384.48260498046875,
            y1: 358.4652099609375,
            y2: 376.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 304.74713134765625,
              x2: 344.48260498046875,
              y1: 358.4652099609375,
              y2: 376.9652099609375,
            },
          ],
        },
        redactionType: { id: "1", name: "Named individual" },
        textContent: ": Com",
        type: "searchPII",
      },
      {
        groupId: "1",
        piiCategory: "Person",
        redactionStatus: "accepted",
        highlightType: "linear",
        id: "1",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 204.74713134765625,
            x2: 284.48260498046875,
            y1: 258.4652099609375,
            y2: 276.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 204.74713134765625,
              x2: 244.48260498046875,
              y1: 258.4652099609375,
              y2: 276.9652099609375,
            },
          ],
        },
        redactionType: { id: "1", name: "Named individual" },
        textContent: ": Com",
        type: "searchPII",
      },
      {
        groupId: "2",
        piiCategory: "Occupation",
        redactionStatus: "ignored",
        highlightType: "linear",
        id: "2",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 304.74713134765625,
            x2: 384.48260498046875,
            y1: 358.4652099609375,
            y2: 376.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 304.74713134765625,
              x2: 384.48260498046875,
              y1: 358.4652099609375,
              y2: 376.9652099609375,
            },
          ],
        },
        redactionType: { id: "2", name: "Other" },
        textContent: ": Com",
        type: "searchPII",
      },
    ] as ISearchPIIHighlight[];

    const result = mapSearchPIISaveRedactionObject(
      manualHighlight,
      suggestedHighlights
    );

    expect(result).toEqual({
      categories: [
        {
          countAccepted: 1,
          countAmended: 1,
          countSuggestions: 2,
          polarisCategory: "Named individual",
          providerCategory: "Person",
        },
        {
          countAccepted: 0,
          countAmended: 0,
          countSuggestions: 1,
          polarisCategory: "Other",
          providerCategory: "Occupation",
        },
      ],
    });
  });

  it("Only ignored highlight should be considered in the amended count calculation", () => {
    const manualHighlight = [
      {
        highlightType: "linear",
        id: "1717577678814-0",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 304.74713134765625,
            x2: 344.48260498046875,
            y1: 358.4652099609375,
            y2: 376.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 304.74713134765625,
              x2: 344.48260498046875,
              y1: 358.4652099609375,
              y2: 376.9652099609375,
            },
          ],
        },
        redactionType: { id: "1", name: "Named individual" },
        textContent: ": Com",
        type: "redaction",
      },
    ] as IPdfHighlight[];

    const suggestedHighlights = [
      {
        groupId: "1",
        piiCategory: "Person",
        redactionStatus: "acceptedAll",
        highlightType: "linear",
        id: "1",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 304.74713134765625,
            x2: 384.48260498046875,
            y1: 358.4652099609375,
            y2: 376.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 304.74713134765625,
              x2: 344.48260498046875,
              y1: 358.4652099609375,
              y2: 376.9652099609375,
            },
          ],
        },
        redactionType: { id: "1", name: "Named individual" },
        textContent: ": Com",
        type: "searchPII",
      },
      {
        groupId: "1",
        piiCategory: "Person",
        redactionStatus: "accepted",
        highlightType: "linear",
        id: "1",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 204.74713134765625,
            x2: 284.48260498046875,
            y1: 258.4652099609375,
            y2: 276.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 204.74713134765625,
              x2: 244.48260498046875,
              y1: 258.4652099609375,
              y2: 276.9652099609375,
            },
          ],
        },
        redactionType: { id: "1", name: "Named individual" },
        textContent: ": Com",
        type: "searchPII",
      },
      {
        groupId: "2",
        piiCategory: "Occupation",
        redactionStatus: "ignored",
        highlightType: "linear",
        id: "2",
        position: {
          boundingRect: {
            height: 1122.8994154404354,
            pageNumber: 1,
            width: 794.0000000000001,
            x1: 304.74713134765625,
            x2: 384.48260498046875,
            y1: 358.4652099609375,
            y2: 376.9652099609375,
          },

          pageNumber: 1,
          rects: [
            {
              height: 1122.8994154404354,
              pageNumber: 1,
              width: 794.0000000000001,
              x1: 304.74713134765625,
              x2: 384.48260498046875,
              y1: 358.4652099609375,
              y2: 376.9652099609375,
            },
          ],
        },
        redactionType: { id: "2", name: "Other" },
        textContent: ": Com",
        type: "searchPII",
      },
    ] as ISearchPIIHighlight[];

    const result = mapSearchPIISaveRedactionObject(
      manualHighlight,
      suggestedHighlights
    );

    expect(result).toEqual({
      categories: [
        {
          countAccepted: 2,
          countAmended: 0,
          countSuggestions: 2,
          polarisCategory: "Named individual",
          providerCategory: "Person",
        },
        {
          countAccepted: 0,
          countAmended: 1,
          countSuggestions: 1,
          polarisCategory: "Other",
          providerCategory: "Occupation",
        },
      ],
    });
  });
});
