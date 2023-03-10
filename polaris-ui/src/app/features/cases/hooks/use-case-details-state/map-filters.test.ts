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
          cmsDocType: { code: "code-x", name: undefined },
          presentationCategory: "category-1",
        },
        {
          cmsDocType: { code: "code-c", name: "doc-type-b" },
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
          label: "doc-type-b",
        },
        "code-x": {
          count: 1,
          isSelected: false,
          label: "Unknown",
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
          cmsDocType: { code: "code-c", name: "doc-type-c" },
          presentationCategory: "category-3",
        },
        {
          cmsDocType: { code: "code-c", name: "doc-type-b" },
          presentationCategory: "category-1",
        },
        {
          cmsDocType: { code: "code-b", name: "doc-type-c" },
          presentationCategory: "category-2",
        },
        {
          cmsDocType: { code: "code-a", name: "doc-type-a" },
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
          label: "doc-type-a",
        },
        "code-b": {
          count: 1,
          isSelected: false,
          label: "doc-type-c",
        },
        "code-c": {
          count: 2,
          isSelected: false,
          label: "doc-type-b",
        },
      },
    });
  });
});
