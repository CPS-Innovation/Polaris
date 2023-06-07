import { mapAccordionState } from "./map-accordion-state";

import { ApiResult } from "../../../../common/types/ApiResult";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

jest.mock("./document-category-definitions", () => ({
  categoryNamesInPresentationOrder: ["category-a", "category-b"],
}));

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

  it("can map from an api result to accordion input shape", () => {
    const apiResult: ApiResult<MappedCaseDocument[]> = {
      status: "succeeded",
      data: [
        {
          documentId: "1",
          cmsDocumentId: "1",
          presentationCategory: "category-a",
          cmsOriginalFileName: "foo",
          presentationTitle: "foo!",
          cmsMimeType: "application/pdf",
          presentationFileName: "foo!",
          polarisDocumentVersionId: 1,
          cmsDocType: {
            documentTypeId: 1,
            documentType: "MG11",
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
          presentationCategory: "category-b",
          cmsOriginalFileName: "bar",
          presentationTitle: "bar!",
          cmsMimeType: "application/pdf",
          presentationFileName: "bar!",
          polarisDocumentVersionId: 1,
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
          sectionId: "category-a",
          sectionLabel: "category-a",
          docs: [
            {
              documentId: "1",
              cmsDocumentId: "1",
              presentationCategory: "category-a",
              cmsOriginalFileName: "foo",
              presentationTitle: "foo!",
              cmsMimeType: "application/pdf",
              presentationFileName: "foo!",
              polarisDocumentVersionId: 1,
              cmsDocType: {
                documentTypeId: 1,
                documentType: "MG11",
                documentCategory: "MGForm",
              },
              cmsFileCreatedDate: "2020-01-01",
              presentationFlags: {
                read: "Ok",
                write: "Ok",
              },
            },
          ],
        },
        {
          sectionId: "category-b",
          sectionLabel: "category-b",
          docs: [
            {
              documentId: "2",
              cmsDocumentId: "2",
              presentationCategory: "category-b",
              cmsOriginalFileName: "bar",
              presentationTitle: "bar!",
              cmsMimeType: "application/pdf",
              presentationFileName: "bar!",
              polarisDocumentVersionId: 1,
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
          presentationCategory: "category-a",
          cmsOriginalFileName: "foo",
          presentationTitle: "foo!",
          cmsMimeType: "application/pdf",
          presentationFileName: "foo!",
          polarisDocumentVersionId: 1,
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
          presentationCategory: "category-b",
          cmsOriginalFileName: "bar",
          presentationTitle: "bar!",
          cmsMimeType: "application/pdf",
          presentationFileName: "bar!",
          polarisDocumentVersionId: 1,
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
          sectionId: "category-a",
          sectionLabel: "category-a",
          docs: [],
        },
        {
          sectionId: "category-b",
          sectionLabel: "category-b",
          docs: [
            {
              documentId: "2",
              cmsDocumentId: "2",
              presentationCategory: "category-b",
              cmsOriginalFileName: "bar",
              presentationTitle: "bar!",
              cmsMimeType: "application/pdf",
              presentationFileName: "bar!",
              polarisDocumentVersionId: 1,
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
      ],
    } as ReturnType<typeof mapAccordionState>);
  });
});
