import {
  getNormalizedRedactionHighlights,
  roundToFixedDecimalPlaces,
} from "./redactionUtils";

describe("redactionUtils", () => {
  describe("getNormalizedRedactionHighlights", () => {
    it("should return correct normalized highlight data for a smaller canvas height", () => {
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
      ];

      const expectedResult = [
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 1142.86,
              height: 800,
              x1: 114.29,
              y1: 114.29,
              x2: 171.43,
              y2: 171.43,
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
      ];
      expect(
        getNormalizedRedactionHighlights(redactionHighlights, 800)
      ).toEqual(expectedResult);
    });
    it("should return correct normalized highlight data for a bigger canvas height", () => {
      const redactionHighlights = [
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 1000,
              height: 900,
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
      ];

      const expectedResult = [
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 888.89,
              height: 800,
              x1: 88.89,
              y1: 88.89,
              x2: 133.33,
              y2: 133.33,
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
      ];
      expect(
        getNormalizedRedactionHighlights(redactionHighlights, 800)
      ).toEqual(expectedResult);
    });

    it("should return correct normalized highlight data for multiple redactionHighlights ", () => {
      const redactionHighlights = [
        {
          highlightType: "area" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 700,
              height: 600,
              x1: 10,
              y1: 10,
              x2: 20,
              y2: 30,
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
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 900,
              height: 1000,
              x1: 10,
              y1: 10,
              x2: 20,
              y2: 30,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 1,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 1000,
              height: 700,
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
          redactionAddedOrder: 2,
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
              width: 1073.33,
              height: 920,
              x1: 15.33,
              y1: 15.33,
              x2: 30.67,
              y2: 46,
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
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 828,
              height: 920,
              x1: 9.2,
              y1: 9.2,
              x2: 18.4,
              y2: 27.6,
            },
            rects: [],
            pageNumber: 1,
          },
          redactionAddedOrder: 1,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              height: 920,
              width: 1314.29,
              x1: 131.43,
              x2: 197.14,
              y1: 131.43,
              y2: 197.14,
            },
            rects: [
              {
                height: 920,
                width: 1314.29,
                x1: 131.43,
                x2: 197.14,
                y1: 131.43,
                y2: 197.14,
              },
              {
                width: 1314.29,
                height: 920,
                x1: 144.57,
                y1: 144.57,
                x2: 210.29,
                y2: 210.29,
              },
            ],
            pageNumber: 1,
          },
          redactionAddedOrder: 2,
          redactionType: { id: "1", name: "Named individual" },
          textContent:
            "This is an area redaction and redacted content is unavailable",
          type: "redaction" as "redaction" | "search",
        },
      ];
      expect(
        getNormalizedRedactionHighlights(redactionHighlights, 920)
      ).toEqual(expectedResult);
    });

    it("should return correct normalized highlight data, for highlight type linear", () => {
      const redactionHighlights = [
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788147",
          position: {
            boundingRect: {
              width: 1000,
              height: 700,
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
      ];

      const expectedResult = [
        {
          highlightType: "linear" as "area" | "linear",
          id: "1707153788147",
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
      ];
      expect(
        getNormalizedRedactionHighlights(redactionHighlights, 800)
      ).toEqual(expectedResult);
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
