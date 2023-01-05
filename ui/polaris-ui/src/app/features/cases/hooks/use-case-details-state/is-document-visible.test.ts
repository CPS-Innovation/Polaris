import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import { isDocumentVisible } from "./is-document-visible";

describe("isDocumentVisible", () => {
  it("can indicate a document is visible if no filters are set", () => {
    const result = isDocumentVisible(
      {
        cmsDocType: { code: "foo" },
        isVisible: true,
      } as MappedDocumentResult,
      {
        docType: {
          foo: { isSelected: false, count: 1, label: "" },
          bar: { isSelected: false, count: 1, label: "" },
        },
        category: {
          foo: { isSelected: false, count: 1, label: "" },
          bar: { isSelected: false, count: 1, label: "" },
        },
      }
    );

    expect(result).toEqual({ isVisible: true, hasChanged: false });
  });

  it("can indicate a document is visible its docType is set to be visible", () => {
    const result = isDocumentVisible(
      {
        cmsDocType: { code: "foo" },
        isVisible: true,
      } as MappedDocumentResult,
      {
        docType: { foo: { isSelected: true, count: 1, label: "" } },
        category: {},
      }
    );

    expect(result).toEqual({ isVisible: true, hasChanged: false });
  });

  it("can indicate a document is visible its category is set to be visible", () => {
    const result = isDocumentVisible(
      {
        cmsDocType: { code: "foo" },
        presentationCategory: "bar",
        isVisible: true,
      } as MappedDocumentResult,
      {
        docType: {},
        category: { bar: { isSelected: true, count: 1, label: "" } },
      }
    );

    expect(result).toEqual({ isVisible: true, hasChanged: false });
  });

  it("can indicate a document is not visible", () => {
    const result = isDocumentVisible(
      {
        cmsDocType: { code: "foo" },
        presentationCategory: "bar",
        isVisible: false,
      } as MappedDocumentResult,
      {
        docType: {},
        category: { baz: { isSelected: true, count: 1, label: "" } },
      }
    );

    expect(result).toEqual({ isVisible: false, hasChanged: false });
  });

  it("can indicate a document has changed visibility", () => {
    const result = isDocumentVisible(
      {
        cmsDocType: { code: "foo" },
        presentationCategory: "bar",
        isVisible: true,
      } as MappedDocumentResult,
      {
        docType: {},
        category: { baz: { isSelected: true, count: 1, label: "" } },
      }
    );

    expect(result).toEqual({ isVisible: false, hasChanged: true });
  });
});
