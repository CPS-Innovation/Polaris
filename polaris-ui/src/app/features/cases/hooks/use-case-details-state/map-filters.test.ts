import { mapFilters } from "./map-filters";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";

jest.mock("./document-category-definitions", () => ({
  categoryNamesInPresentationOrder: ["category-1", "category-2", "category-3"],
}));

describe("mapFilters", () => {
  it("can present a human readable label for an unknown category", () => {
    const input = {
      totalOccurrencesCount: -1,
      filteredOccurrencesCount: -1,
      filteredDocumentCount: -1,
      documentResults: [
        {
          cmsDocType: { documentType: "code-x" },
          presentationCategory: "category-1",
        },
        {
          cmsDocType: { documentType: "code-c" },
          presentationCategory: "category-1",
        },
      ],
    } as MappedTextSearchResult;

    const result = mapFilters(input);

    expect(result).toEqual({
      category: {
        "category-1": {
          count: 2,
          isSelected: false,
          label: "category-1",
        },
        "category-2": {
          count: 0,
          isSelected: false,
          label: "category-2",
        },
        "category-3": {
          count: 0,
          isSelected: false,
          label: "category-3",
        },
      },
      docType: {
        "code-c": {
          count: 1,
          isSelected: false,
          label: "code-c",
        },
        "code-x": {
          count: 1,
          isSelected: false,
          label: "code-x",
        },
      },
    });
  });

  it("can map filters from a MappedTextSearchResult", () => {
    const input = {
      totalOccurrencesCount: -1,
      filteredOccurrencesCount: -1,
      filteredDocumentCount: -1,
      documentResults: [
        {
          cmsDocType: { documentType: "code-c" },
          presentationCategory: "category-3",
        },
        {
          cmsDocType: { documentType: "code-c" },
          presentationCategory: "category-1",
        },
        {
          cmsDocType: { documentType: "code-b" },
          presentationCategory: "category-2",
        },
        {
          cmsDocType: { documentType: "code-a" },
          presentationCategory: "category-1",
        },
      ],
    } as MappedTextSearchResult;

    const result = mapFilters(input);

    expect(result).toEqual({
      category: {
        "category-1": {
          count: 2,
          isSelected: false,
          label: "category-1",
        },
        "category-2": {
          count: 1,
          isSelected: false,
          label: "category-2",
        },
        "category-3": {
          count: 1,
          isSelected: false,
          label: "category-3",
        },
      },
      docType: {
        "code-a": {
          count: 1,
          isSelected: false,
          label: "code-a",
        },
        "code-b": {
          count: 1,
          isSelected: false,
          label: "code-b",
        },
        "code-c": {
          count: 2,
          isSelected: false,
          label: "code-c",
        },
      },
    });
  });
});
