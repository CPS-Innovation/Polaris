import {
  getWordStartingIndices,
  getNonEmptyTextContentElements,
} from "./useDocumentFocusHelpers";

describe("useDocumentFocus helpers", () => {
  describe("getWordStartingIndices", () => {
    it("should return starting index of words in a sentence", () => {
      const startingIndex = getWordStartingIndices("should return starting");
      expect(startingIndex).toEqual([0, 7, 14]);
    });
    it("should return starting index of words in a sentence", () => {
      const startingIndex = getWordStartingIndices("this is a test");
      expect(startingIndex).toEqual([0, 5, 8, 10]);
    });
    it("should return starting index of words in a sentence with more spaces", () => {
      const startingIndex = getWordStartingIndices(
        "should    return   starting"
      );
      expect(startingIndex).toEqual([0, 10, 19]);
    });
  });

  describe("getNonEmptyTextContentElements", () => {
    it("should return filtered non-empty text leaf child elements", () => {
      const inputSpanElements = [
        {
          classList: { contains: () => false },
          children: [
            { id: "1", textContent: "" },
            { id: "2", textContent: "abc" },
          ],
        },
        {
          classList: { contains: () => true },
          textContent: "efg",
          children: [],
        },
        {
          classList: { contains: () => false },
          children: [
            { id: "1", textContent: "123" },
            { id: "2", textContent: "345" },
          ],
        },
        { classList: { contains: () => false }, textContent: "", children: [] },
        {
          classList: { contains: () => false },
          textContent: "def",
          children: [],
        },
      ] as unknown as HTMLCollection;
      const nonEmptyContents =
        getNonEmptyTextContentElements(inputSpanElements);
      expect(JSON.stringify(nonEmptyContents)).toEqual(
        JSON.stringify([
          { id: "2", textContent: "abc" },
          { id: "1", textContent: "123" },
          { id: "2", textContent: "345" },
          {
            classList: { contains: () => false },
            textContent: "def",
            children: [],
          },
        ])
      );
    });
  });
});
