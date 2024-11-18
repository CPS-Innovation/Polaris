import {
  getNormalizedRedactionHighlights,
  roundToFixedDecimalPlaces,
} from "./redactionUtils";

describe("redactionUtils", () => {
  describe("getNormalizedRedactionHighlights", () => {
    it("Should not normalize if there is no resize (height remains same)", () => {
      const redactionHighlights = [
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              height: 700,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788148",
          position: {
            boundingRect: {
              height: 700,
              width: 1000,
              x1: 200,
              y1: 200,
              x2: 250,
              y2: 250,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 800,
              width: 1100,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
      ];

      expect(getNormalizedRedactionHighlights(redactionHighlights)).toEqual(
        redactionHighlights
      );
    });
    it("should return correct normalized highlight data for resized page redactions", () => {
      const redactionHighlights = [
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              height: 700,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788148",
          position: {
            boundingRect: {
              height: 900,
              width: 1000,
              x1: 200,
              y1: 200,
              x2: 250,
              y2: 250,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 800,
              width: 1100,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 1000,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
      ];

      const expectedResult = [
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              height: 700,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788148",
          position: {
            boundingRect: {
              height: 700,
              width: 777.78,
              x1: 155.56,
              y1: 155.56,
              x2: 194.44,
              y2: 194.44,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 800,
              width: 1100,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 800,
              width: 800,
              x1: 80,
              y1: 80,
              x2: 120,
              y2: 120,
            },
            rects: [],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
      ];
      expect(getNormalizedRedactionHighlights(redactionHighlights)).toEqual(
        expectedResult
      );
    });
    it("should return correct normalized highlight data, for highlight type linear", () => {
      const redactionHighlights = [
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              height: 800,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [
              {
                width: 800,
                height: 1000,
                x1: 100,
                y1: 100,
                x2: 150,
                y2: 150,
              },
              {
                width: 800,
                height: 1000,
                x1: 110,
                y1: 110,
                x2: 160,
                y2: 160,
              },
            ],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788148",
          position: {
            boundingRect: {
              height: 700,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [
              {
                width: 1000,
                height: 700,
                x1: 100,
                y1: 100,
                x2: 150,
                y2: 150,
              },
              {
                width: 1000,
                height: 700,
                x1: 110,
                y1: 110,
                x2: 160,
                y2: 160,
              },
            ],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 700,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 900,
              width: 1000,
              x1: 200,
              y1: 200,
              x2: 250,
              y2: 250,
            },
            rects: [
              {
                height: 900,
                width: 1000,
                x1: 200,
                y1: 200,
                x2: 250,
                y2: 250,
              },
            ],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
      ];

      const expectedResult = [
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              height: 800,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [
              {
                width: 800,
                height: 1000,
                x1: 100,
                y1: 100,
                x2: 150,
                y2: 150,
              },
              {
                width: 800,
                height: 1000,
                x1: 110,
                y1: 110,
                x2: 160,
                y2: 160,
              },
            ],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788148",
          position: {
            boundingRect: {
              width: 1142.86,
              height: 800,
              x1: 114.29,
              y1: 114.29,
              x2: 171.43,
              y2: 171.43,
            },
            rects: [
              {
                width: 1142.86,
                height: 800,
                x1: 114.29,
                y1: 114.29,
                x2: 171.43,
                y2: 171.43,
              },
              {
                width: 1142.86,
                height: 800,
                x1: 125.71,
                y1: 125.71,
                x2: 182.86,
                y2: 182.86,
              },
            ],
            pageNumber: 1,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 700,
              width: 1000,
              x1: 100,
              y1: 100,
              x2: 150,
              y2: 150,
            },
            rects: [],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788149",
          position: {
            boundingRect: {
              height: 700,
              width: 777.78,
              x1: 155.56,
              y1: 155.56,
              x2: 194.44,
              y2: 194.44,
            },
            rects: [
              {
                height: 700,
                width: 777.78,
                x1: 155.56,
                y1: 155.56,
                x2: 194.44,
                y2: 194.44,
              },
            ],
            pageNumber: 2,
          },
          redactionAddedOrder: 0,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
      ];
      expect(getNormalizedRedactionHighlights(redactionHighlights)).toEqual(
        expectedResult
      );
    });
  });
  describe("roundToFixedDecimalPlaces", () => {
    it("should round the number with given decimal places", () => {
      expect(roundToFixedDecimalPlaces(100.12245)).toEqual(100.12);
      expect(roundToFixedDecimalPlaces(100.34645)).toEqual(100.35);
      expect(roundToFixedDecimalPlaces(6.971895, 3)).toEqual(6.972);
    });
  });
});
