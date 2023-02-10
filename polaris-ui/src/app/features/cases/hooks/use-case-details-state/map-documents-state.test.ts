import { AsyncResult } from "../../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { mapDocumentsState } from "./map-documents-state";
import * as documentCategoryDefinitions from "./document-category-definitions";
import * as getFileNameWithoutExtension from "../../logic/get-file-name-without-extension";
import { PresentationDocumentProperties } from "../../domain/PdfDocument";

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
      documentId: 0,
      fileName: "foo",
    } as PresentationDocumentProperties;
    const doc2 = {
      documentId: 1,
      fileName: "bar",
    } as PresentationDocumentProperties;

    const input = [doc1, doc2];

    const expectedResult = {
      status: "succeeded",
      data: [
        {
          ...doc1,
          tabSafeId: "d0",
          presentationCategory: "category0",
          fileName: "foo",
          presentationFileName: "foo!",
        },
        {
          ...doc2,
          tabSafeId: "d1",
          presentationCategory: "category1",
          fileName: "bar",
          presentationFileName: "bar!",
        },
      ] as MappedCaseDocument[],
    };

    const result = mapDocumentsState(input);

    expect(result).toEqual(expectedResult);
  });
});
