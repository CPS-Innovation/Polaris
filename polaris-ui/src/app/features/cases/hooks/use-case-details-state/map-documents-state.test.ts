import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { mapDocumentsState } from "./map-documents-state";
import * as documentCategoryDefinitions from "./document-category-definitions";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

describe("mapDocumentsState", () => {
  beforeEach(() => {
    jest
      .spyOn(documentCategoryDefinitions, "getCategory")
      .mockImplementation((item: PresentationDocumentProperties) => ({
        category: "category" + item.documentId,
        subCategory: undefined,
      }));
  });

  it("can map CaseDocuments to MappedCaseDocuments", () => {
    const doc1 = {
      documentId: "0",
      presentationTitle: "foo",
    } as PresentationDocumentProperties;
    const doc2 = {
      documentId: "1",
      presentationTitle: "bar",
    } as PresentationDocumentProperties;

    const input = [doc1, doc2];

    const expectedResult = {
      status: "succeeded",
      data: [
        {
          ...doc1,
          attachments: [],
          presentationCategory: "category0",
          presentationFileName: "foo",
          presentationSubCategory: undefined,
        },
        {
          ...doc2,
          attachments: [],
          presentationCategory: "category1",
          presentationFileName: "bar",
          presentationSubCategory: undefined,
        },
      ] as MappedCaseDocument[],
    };

    const result = mapDocumentsState(input);

    expect(result).toEqual(expectedResult);
  });
});
