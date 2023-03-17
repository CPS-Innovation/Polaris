import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { mapDocumentsState } from "./map-documents-state";
import * as documentCategoryDefinitions from "./document-category-definitions";
import * as getFileNameWithoutExtension from "../../logic/get-file-name-without-extension";
import { PresentationDocumentProperties } from "../../domain/PipelineDocument";

describe("mapDocumentsState", () => {
  beforeEach(() => {
    jest
      .spyOn(documentCategoryDefinitions, "getCategory")
      .mockImplementation(
        (item: PresentationDocumentProperties) => "category" + item.documentId
      );

    jest
      .spyOn(getFileNameWithoutExtension, "getFileNameWithoutExtension")
      .mockImplementation((filename: string | undefined) => filename + "!");
  });

  it("can map CaseDocuments to MappedCaseDocuments", () => {
    const doc1 = {
      documentId: "0",
      cmsOriginalFileName: "foo",
    } as PresentationDocumentProperties;
    const doc2 = {
      documentId: "1",
      cmsOriginalFileName: "bar",
    } as PresentationDocumentProperties;

    const input = [doc1, doc2];

    const expectedResult = {
      status: "succeeded",
      data: [
        {
          ...doc1,
          presentationCategory: "category0",
          cmsOriginalFileName: "foo",
          presentationFileName: "foo!",
        },
        {
          ...doc2,
          presentationCategory: "category1",
          cmsOriginalFileName: "bar",
          presentationFileName: "bar!",
        },
      ] as MappedCaseDocument[],
    };

    const result = mapDocumentsState(input);

    expect(result).toEqual(expectedResult);
  });
});
