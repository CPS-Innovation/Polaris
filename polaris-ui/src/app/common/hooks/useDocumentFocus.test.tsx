import { getWordStartingIndices } from "./useDocumentFocusHelpers";

describe("useDocumentFocus helpers", () => {
  it("should return starting index of words in a sentence", () => {
    const startingIndex = getWordStartingIndices("should return starting");
    expect(startingIndex).toEqual([0, 7, 14]);
  });
  it("should return starting index of words in a sentence", () => {
    const startingIndex = getWordStartingIndices("should    return   starting");
    expect(startingIndex).toEqual([0, 10, 19]);
  });
});
