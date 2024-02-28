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
        subCategory: null,
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
          presentationSubCategory: null,
          witnessIndicators: [],
        },
        {
          ...doc2,
          attachments: [],
          presentationCategory: "category1",
          presentationFileName: "bar",
          presentationSubCategory: null,
          witnessIndicators: [],
        },
      ] as MappedCaseDocument[],
    };

    const result = mapDocumentsState(input, [], 123);

    expect(result).toEqual(expectedResult);
  });

  it("can map CaseDocuments to MappedCaseDocuments with witnessIndicators", () => {
    const doc1 = {
      documentId: "0",
      presentationTitle: "foo",
      witnessId: 2762766,
    } as PresentationDocumentProperties;
    const doc2 = {
      documentId: "1",
      presentationTitle: "bar",
    } as PresentationDocumentProperties;

    const input = [doc1, doc2];

    const witnessesInput = [
      {
        id: 2762766,
        shoulderNumber: null,
        title: "Prof",
        name: "John Doe",
        hasStatements: true,
        listOrder: 1,
        child: false,
        expert: false,
        greatestNeed: false,
        prisoner: false,
        interpreter: false,
        vulnerable: false,
        police: false,
        professional: false,
        specialNeeds: false,
        intimidated: false,
        victim: true,
      },
    ];

    const expectedResult = {
      status: "succeeded",
      data: [
        {
          ...doc1,
          attachments: [],
          presentationCategory: "category0",
          presentationFileName: "foo",
          docRead: false,
          presentationSubCategory: null,
          witnessIndicators: ["V"],
        },
        {
          ...doc2,
          attachments: [],
          presentationCategory: "category1",
          presentationFileName: "bar",
          presentationSubCategory: null,
          witnessIndicators: [],
        },
      ] as MappedCaseDocument[],
    };

    const result = mapDocumentsState(input, witnessesInput, 123);

    expect(result).toEqual(expectedResult);
  });
});
