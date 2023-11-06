import { mapAccordionState } from "./map-accordion-state";
import { ApiResult } from "../../../../common/types/ApiResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

const mapUnSortedDocsToCategory = (
  categories: string[],
  docs: Record<string, any>[]
) => {
  return categories.reduce((acc, category) => {
    const newDocs = docs.map((document) => ({
      ...document,
      presentationCategory: category,
    }));

    return [...acc, ...newDocs];
  }, [] as any);
};

const mapSortedDocToCategory = (
  docs: Record<string, any>[],
  category: string
) => {
  const newDocs = docs.map((document) => ({
    ...document,
    presentationCategory: category,
  }));

  return newDocs;
};

describe("mapAccordionState", () => {
  it("can return a loading status when the api result is  'loading'", () => {
    const apiResult: ApiResult<PresentationDocumentProperties[]> = {
      status: "loading",
    };

    const result = mapAccordionState(apiResult);

    expect(result).toEqual({ status: "loading" });
  });

  it("can return a loading status when the api is result is 'failed'", () => {
    const apiResult: ApiResult<PresentationDocumentProperties[]> = {
      status: "failed",
      error: null,
      httpStatusCode: undefined,
    };

    const result = mapAccordionState(apiResult);

    expect(result).toEqual({ status: "loading" });
  });

  it("can map from an api result to accordion input shape and sort it correctly based on cmsFileCreatedDate ascending (oldest first)", () => {
    const sortByCmsFileCreatedDateCategories: any = [
      "Reviews",
      "Forensics",
      "Defendant",
      "Communications",
      "Uncategorised",
    ];
    const rawUnSortedDocuments = [
      {
        documentId: "1",
        cmsDocType: {
          documentTypeId: 1,
          documentType: "MG11",
        },
        cmsFileCreatedDate: "2020-01-03",
      },
      {
        documentId: "2",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG12",
        },
        cmsFileCreatedDate: "2020-01-01",
      },
      {
        documentId: "3",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG12",
        },
        cmsFileCreatedDate: "2020-01-02",
      },
    ];
    const sortedDocuments = [
      {
        documentId: "2",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG12",
        },
        cmsFileCreatedDate: "2020-01-01",
      },
      {
        documentId: "3",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG12",
        },
        cmsFileCreatedDate: "2020-01-02",
      },
      {
        documentId: "1",
        cmsDocType: {
          documentTypeId: 1,
          documentType: "MG11",
        },
        cmsFileCreatedDate: "2020-01-03",
      },
    ];

    const apiResult: ApiResult<MappedCaseDocument[]> = {
      status: "succeeded",
      data: mapUnSortedDocsToCategory(
        sortByCmsFileCreatedDateCategories,
        rawUnSortedDocuments
      ) as MappedCaseDocument[],
    };

    const result = mapAccordionState(apiResult);

    expect(result).toEqual({
      status: "succeeded",
      data: [
        {
          sectionId: "Reviews",
          sectionLabel: "Reviews",
          docs: mapSortedDocToCategory(sortedDocuments, "Reviews"),
        },
        {
          sectionId: "Case overview",
          sectionLabel: "Case overview",
          docs: [],
        },
        {
          sectionId: "Statements",
          sectionLabel: "Statements",
          docs: [],
        },
        {
          sectionId: "Exhibits",
          sectionLabel: "Exhibits",
          docs: [],
        },
        {
          sectionId: "Forensics",
          sectionLabel: "Forensics",
          docs: mapSortedDocToCategory(sortedDocuments, "Forensics"),
        },
        {
          sectionId: "Unused material",
          sectionLabel: "Unused material",
          docs: [],
        },
        {
          sectionId: "Defendant",
          sectionLabel: "Defendant",
          docs: mapSortedDocToCategory(sortedDocuments, "Defendant"),
        },
        {
          sectionId: "Court preparation",
          sectionLabel: "Court preparation",
          docs: [],
        },
        {
          sectionId: "Communications",
          sectionLabel: "Communications",
          docs: mapSortedDocToCategory(sortedDocuments, "Communications"),
        },
        {
          sectionId: "Uncategorised",
          sectionLabel: "Uncategorised",
          docs: mapSortedDocToCategory(sortedDocuments, "Uncategorised"),
        },
      ],
    } as ReturnType<typeof mapAccordionState>);
  });

  it("can map from an api result to accordion input shape and sort it correctly based on documentType and cmsFileCreatedDate ascending ", () => {
    const sortCategories: string[] = [
      "Case overview",
      "Unused material",
      "Court preparation",
    ];
    const rawUnSortedDocuments = [
      {
        documentId: "1",
        cmsDocType: {
          documentTypeId: 1,
          documentType: "MG 12A",
        },
        cmsFileCreatedDate: "2020-01-03",
      },
      {
        documentId: "2",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG12",
        },
        cmsFileCreatedDate: "2020-01-01",
      },
      {
        documentId: "3",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG 7A",
        },
        cmsFileCreatedDate: "2020-01-02",
      },
    ];
    const sortedDocuments = [
      {
        documentId: "3",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG 7A",
        },
        cmsFileCreatedDate: "2020-01-02",
      },
      {
        documentId: "2",
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG12",
        },
        cmsFileCreatedDate: "2020-01-01",
      },
      {
        documentId: "1",
        cmsDocType: {
          documentTypeId: 1,
          documentType: "MG 12A",
        },
        cmsFileCreatedDate: "2020-01-03",
      },
    ];

    const apiResult: ApiResult<MappedCaseDocument[]> = {
      status: "succeeded",
      data: mapUnSortedDocsToCategory(
        sortCategories,
        rawUnSortedDocuments
      ) as MappedCaseDocument[],
    };

    const result = mapAccordionState(apiResult);

    expect(result).toEqual({
      status: "succeeded",
      data: [
        {
          sectionId: "Reviews",
          sectionLabel: "Reviews",
          docs: [],
        },
        {
          sectionId: "Case overview",
          sectionLabel: "Case overview",
          docs: mapSortedDocToCategory(sortedDocuments, "Case overview"),
        },
        {
          sectionId: "Statements",
          sectionLabel: "Statements",
          docs: [],
        },
        {
          sectionId: "Exhibits",
          sectionLabel: "Exhibits",
          docs: [],
        },
        {
          sectionId: "Forensics",
          sectionLabel: "Forensics",
          docs: [],
        },
        {
          sectionId: "Unused material",
          sectionLabel: "Unused material",
          docs: mapSortedDocToCategory(sortedDocuments, "Unused material"),
        },
        {
          sectionId: "Defendant",
          sectionLabel: "Defendant",
          docs: [],
        },
        {
          sectionId: "Court preparation",
          sectionLabel: "Court preparation",
          docs: mapSortedDocToCategory(sortedDocuments, "Court preparation"),
        },
        {
          sectionId: "Communications",
          sectionLabel: "Communications",
          docs: [],
        },
        {
          sectionId: "Uncategorised",
          sectionLabel: "Uncategorised",
          docs: [],
        },
      ],
    } as ReturnType<typeof mapAccordionState>);
  });

  it("can map from an api result to accordion input shape and sort it correctly based on categoryListOrder and documentId ascending ", () => {
    const sortCategories: string[] = ["Statements", "Exhibits"];
    const rawUnSortedDocuments = [
      {
        documentId: "1",
        categoryListOrder: 2,
        cmsDocType: {
          documentTypeId: 1,
          documentType: "MG 12",
        },
        cmsFileCreatedDate: "2020-01-03",
      },
      {
        documentId: "2",
        categoryListOrder: 2,
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG 12",
        },
        cmsFileCreatedDate: "2020-01-01",
      },
      {
        documentId: "3",
        categoryListOrder: 1,
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG 7",
        },
        cmsFileCreatedDate: "2020-01-02",
      },
    ];
    const sortedDocuments = [
      {
        documentId: "3",
        categoryListOrder: 1,
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG 7",
        },
        cmsFileCreatedDate: "2020-01-02",
      },
      {
        documentId: "1",
        categoryListOrder: 2,
        cmsDocType: {
          documentTypeId: 1,
          documentType: "MG 12",
        },
        cmsFileCreatedDate: "2020-01-03",
      },
      {
        documentId: "2",
        categoryListOrder: 2,
        cmsDocType: {
          documentTypeId: 2,
          documentType: "MG 12",
        },
        cmsFileCreatedDate: "2020-01-01",
      },
    ];

    const apiResult: ApiResult<MappedCaseDocument[]> = {
      status: "succeeded",
      data: mapUnSortedDocsToCategory(
        sortCategories,
        rawUnSortedDocuments
      ) as MappedCaseDocument[],
    };

    const result = mapAccordionState(apiResult);

    expect(result).toEqual({
      status: "succeeded",
      data: [
        {
          sectionId: "Reviews",
          sectionLabel: "Reviews",
          docs: [],
        },
        {
          sectionId: "Case overview",
          sectionLabel: "Case overview",
          docs: [],
        },
        {
          sectionId: "Statements",
          sectionLabel: "Statements",
          docs: mapSortedDocToCategory(sortedDocuments, "Statements"),
        },
        {
          sectionId: "Exhibits",
          sectionLabel: "Exhibits",
          docs: mapSortedDocToCategory(sortedDocuments, "Exhibits"),
        },
        {
          sectionId: "Forensics",
          sectionLabel: "Forensics",
          docs: [],
        },
        {
          sectionId: "Unused material",
          sectionLabel: "Unused material",
          docs: [],
        },
        {
          sectionId: "Defendant",
          sectionLabel: "Defendant",
          docs: [],
        },
        {
          sectionId: "Court preparation",
          sectionLabel: "Court preparation",
          docs: [],
        },
        {
          sectionId: "Communications",
          sectionLabel: "Communications",
          docs: [],
        },
        {
          sectionId: "Uncategorised",
          sectionLabel: "Uncategorised",
          docs: [],
        },
      ],
    } as ReturnType<typeof mapAccordionState>);
  });
  it("can filter out the DAC doc type, from any accordion category", () => {
    const apiResult: ApiResult<MappedCaseDocument[]> = {
      status: "succeeded",
      data: [
        {
          documentId: "1",
          cmsDocumentId: "1",
          presentationCategory: "Reviews",
          cmsOriginalFileName: "foo",
          presentationTitle: "foo!",
          cmsMimeType: "application/pdf",
          presentationFileName: "foo!",
          polarisDocumentVersionId: 1,
          categoryListOrder: null,
          fileExtension: ".pdf",
          hasAttachments: false,
          parentDocumentId: null,
          cmsDocType: {
            documentTypeId: 1,
            documentType: "DAC",
            documentCategory: "MGForm",
          },
          cmsFileCreatedDate: "2020-01-01",
          presentationFlags: {
            read: "Ok",
            write: "Ok",
          },
        },
        {
          documentId: "2",
          cmsDocumentId: "2",
          presentationCategory: "Reviews",
          cmsOriginalFileName: "bar",
          presentationTitle: "bar!",
          cmsMimeType: "application/pdf",
          presentationFileName: "bar!",
          polarisDocumentVersionId: 1,
          categoryListOrder: null,
          fileExtension: ".pdf",
          hasAttachments: false,
          parentDocumentId: null,
          cmsDocType: {
            documentTypeId: 2,
            documentType: "MG12",
            documentCategory: "MGForm",
          },
          cmsFileCreatedDate: "2020-01-02",
          presentationFlags: {
            read: "Ok",
            write: "Ok",
          },
        },
      ],
    };

    const result = mapAccordionState(apiResult);

    expect(result).toEqual({
      status: "succeeded",
      data: [
        {
          sectionId: "Reviews",
          sectionLabel: "Reviews",
          docs: [
            {
              documentId: "2",
              cmsDocumentId: "2",
              presentationCategory: "Reviews",
              cmsOriginalFileName: "bar",
              presentationTitle: "bar!",
              cmsMimeType: "application/pdf",
              presentationFileName: "bar!",
              polarisDocumentVersionId: 1,
              categoryListOrder: null,
              fileExtension: ".pdf",
              hasAttachments: false,
              parentDocumentId: null,
              cmsDocType: {
                documentTypeId: 2,
                documentType: "MG12",
                documentCategory: "MGForm",
              },
              cmsFileCreatedDate: "2020-01-02",
              presentationFlags: {
                read: "Ok",
                write: "Ok",
              },
            },
          ],
        },
        {
          sectionId: "Case overview",
          sectionLabel: "Case overview",
          docs: [],
        },
        {
          sectionId: "Statements",
          sectionLabel: "Statements",
          docs: [],
        },
        {
          sectionId: "Exhibits",
          sectionLabel: "Exhibits",
          docs: [],
        },
        {
          sectionId: "Forensics",
          sectionLabel: "Forensics",
          docs: [],
        },
        {
          sectionId: "Unused material",
          sectionLabel: "Unused material",
          docs: [],
        },
        {
          sectionId: "Defendant",
          sectionLabel: "Defendant",
          docs: [],
        },
        {
          sectionId: "Court preparation",
          sectionLabel: "Court preparation",
          docs: [],
        },
        {
          sectionId: "Communications",
          sectionLabel: "Communications",
          docs: [],
        },
        {
          sectionId: "Uncategorised",
          sectionLabel: "Uncategorised",
          docs: [],
        },
      ],
    } as ReturnType<typeof mapAccordionState>);
  });
});
