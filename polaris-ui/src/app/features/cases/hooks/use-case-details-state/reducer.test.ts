import { CombinedState } from "../../domain/CombinedState";
import { reducer } from "./reducer";
import * as accordionMapper from "./map-accordion-state";
import * as documentsMapper from "./map-documents-state";
import * as apiGateway from "../../api/gateway-api";
import { ApiResult } from "../../../../common/types/ApiResult";
import { PipelineResults } from "../../domain/PipelineResults";
import { AsyncPipelineResult } from "../use-pipeline-api/AsyncPipelineResult";
import * as sorter from "./sort-mapped-text-search-result";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";
import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import * as documentVisibility from "./is-document-visible";
import { ApiTextSearchResult } from "../../domain/ApiTextSearchResult";
import * as textSearchMapper from "./map-text-search";
import * as filters from "./map-filters";
import * as missingDocuments from "./map-missing-documents";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import * as sanitizeSearchTerm from "./sanitizeSearchTerm";

const ERROR = new Error();

describe("useCaseDetailsState reducer", () => {
  afterEach(() => {
    jest.restoreAllMocks();
    jest.useRealTimers();
  });

  describe("UPDATE_CASE_DETAILS", () => {
    it("throws if update case details fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_CASE_DETAILS",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
          },
        })
      ).toThrowError(ERROR);
    });

    it("can update case details", () => {
      const newCaseState = {} as CombinedState["caseState"];

      const nextState = reducer({} as CombinedState, {
        type: "UPDATE_CASE_DETAILS",
        payload: newCaseState,
      });

      expect(nextState.caseState).toBe(newCaseState);
    });
  });

  // describe("UPDATE_CASE_DOCUMENTS", () => {
  //   it("throws if update case documents fails", () => {
  //     expect(() =>
  //       reducer({} as CombinedState, {
  //         type: "UPDATE_CASE_DOCUMENTS",
  //         payload: {
  //           status: "failed",
  //           error: ERROR,
  //           httpStatusCode: undefined,
  //         },
  //       })
  //     ).toThrowError(ERROR);
  //   });

  //   it("can update accordionState if incoming documentsState is ready", () => {
  //     const existingAccordionState = {} as CombinedState["accordionState"];

  //     const newAccordionState = {} as CombinedState["accordionState"];
  //     const newDocumentsState = {
  //       status: "succeeded",
  //     } as CombinedState["documentsState"];

  //     jest
  //       .spyOn(accordionMapper, "mapAccordionState")
  //       .mockImplementation((documentsState) => {
  //         if (documentsState !== newDocumentsState) throw new Error();
  //         return newAccordionState;
  //       });

  //     jest
  //       .spyOn(documentsMapper, "mapDocumentsState")
  //       .mockImplementation((documentsState) => {
  //         if (documentsState !== newDocumentsState) throw new Error();
  //         return newDocumentsState;
  //       });

  //     const nextState = reducer(
  //       { accordionState: existingAccordionState } as CombinedState,
  //       {
  //         type: "UPDATE_CASE_DOCUMENTS",
  //         payload: newDocumentsState,
  //       }
  //     );

  //     expect(nextState.documentsState).toBe(newDocumentsState);
  //     expect(nextState.accordionState).toBe(newAccordionState);
  //   });
  // });

  describe("UPDATE_PIPELINE", () => {
    it("throws if update pipelineState fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
            haveData: false,
          },
        })
      ).toThrowError(ERROR);
    });

    it("can update from pipeline if initiating", () => {
      const existingPipelineState = {
        status: "initiating",
      } as CombinedState["pipelineState"];

      const nextState = reducer(
        {
          pipelineState: existingPipelineState,
        } as CombinedState,
        {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "initiating",
            haveData: false,
          },
        }
      );

      expect(nextState.pipelineState).toEqual(existingPipelineState);
    });

    it("can update from pipeline if succeeded", () => {
      const expectedNextState = {
        status: "incomplete",
        haveData: true,
        data: {
          transactionId: "123",
          status: "Running",
          documents: [{ documentId: 1 }],
        },
      } as AsyncPipelineResult<PipelineResults>;

      const nextState = reducer(
        {
          pipelineState: {},
          documentsState: { status: "succeeded" },
        } as CombinedState,
        {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "incomplete",
            haveData: true,
            data: {
              transactionId: "123",
              status: "Running",
              documents: [{ documentId: 1 }],
            },
          } as AsyncPipelineResult<PipelineResults>,
        }
      );

      expect(nextState.pipelineState).toEqual(expectedNextState);
    });

    it("can update from pipeline if succeeded and is now complete", () => {
      const expectedNextState = {
        status: "complete",
        haveData: true,
        data: {
          documents: [{ documentId: 1, pdfBlobName: "foo" }],
          transactionId: "123",
          status: "Completed",
        },
      } as AsyncPipelineResult<PipelineResults>;

      const nextState = reducer(
        {
          pipelineState: {},
          tabsState: { items: [] },
          documentsState: {
            status: "succeeded",
          },
        } as unknown as CombinedState, // todo: remove the "as unkwon"
        {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "complete",
            haveData: true,
            data: {
              documents: [{ documentId: 1, pdfBlobName: "foo" }],
              status: "Completed",
              transactionId: "123",
            },
          } as AsyncPipelineResult<PipelineResults>,
        }
      );

      expect(nextState.pipelineState).toEqual(expectedNextState);
    });

    it("can update from pipeline when no tabs are open", () => {
      const newPipelineState = {
        status: "complete",
        haveData: true,
        data: {
          documents: [
            {
              documentId: 2,
              pdfBlobName: "foo",
            },
          ],
        },
      } as AsyncPipelineResult<PipelineResults>;

      const existingTabsState = {
        items: [],
        headers: {},
      } as CombinedState["tabsState"];

      const nextState = reducer(
        {
          tabsState: existingTabsState,
          documentsState: { status: "succeeded" },
        } as CombinedState,
        {
          type: "UPDATE_PIPELINE",
          payload: newPipelineState,
        }
      );

      expect(nextState.tabsState).toBe(existingTabsState);
    });

    it("can update from pipeline tabs already open with pdf url", () => {
      const newPipelineState = {
        status: "complete",
        data: {
          documents: [
            {
              documentId: 2,
              pdfBlobName: "foo",
            },
          ],
        },
      } as CombinedState["pipelineState"];

      const existingTabsState = {
        items: [
          { documentId: 1, url: undefined },
          { documentId: 2, url: undefined },
          { documentId: 3, url: undefined },
        ],
      } as CombinedState["tabsState"];

      jest.spyOn(apiGateway, "resolvePdfUrl").mockImplementation((blobName) => {
        if (blobName !== "foo") throw new Error();
        return "baz";
      });

      const nextState = reducer(
        {
          tabsState: existingTabsState,
          documentsState: { status: "succeeded" },
        } as CombinedState,
        {
          type: "UPDATE_PIPELINE",
          payload: newPipelineState,
        }
      );

      expect(nextState.tabsState).toEqual({
        items: [
          { documentId: 1, url: undefined },
          { documentId: 2, url: "baz", pdfBlobName: "foo" },
          { documentId: 3, url: undefined },
        ],
      });
    });
  });

  describe("OPEN_PDF_IN_NEW_TAB", () => {
    it("can set the sasUrl of the expected document", () => {
      const nextState = reducer(
        {
          tabsState: {
            items: [{ documentId: 1 }, { documentId: "bar" }],
          },
        } as CombinedState,
        {
          type: "OPEN_PDF_IN_NEW_TAB",
          payload: {
            pdfId: 1,
            sasUrl: "baz",
          },
        }
      );

      expect(nextState).toEqual({
        tabsState: {
          items: [{ documentId: 1, sasUrl: "baz" }, { documentId: "bar" }],
        },
      });
    });
  });

  describe("OPEN_PDF", () => {
    it("can try to open a tab when the documents are unknown", () => {
      const nextState = reducer(
        {
          documentsState: { status: "loading" },
          tabsState: {
            headers: {
              Authorization: "foo",
              "Correlation-Id": "foo1",
            } as HeadersInit,
          },
        } as CombinedState,
        {
          type: "OPEN_PDF",
          payload: {
            pdfId: 1,
            tabSafeId: "t1",
            mode: "read",
            headers: { Authorization: "bar", "Correlation-Id": "bar1" },
          },
        }
      );

      expect(nextState).toEqual({
        documentsState: { status: "loading" },
        searchState: {
          isResultsVisible: false,
        },
        tabsState: {
          headers: {
            Authorization: "bar",
            "Correlation-Id": "bar1",
          } as HeadersInit,
        },
      } as CombinedState);
    });

    it("can open a tab when the pdf details are known", () => {
      const existingTabsState = {
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [],
      } as CombinedState["tabsState"];

      const existingDocumentsState = {
        status: "succeeded",
        data: [{ documentId: 1 }],
      } as CombinedState["documentsState"];

      const existingPipelineState = {
        status: "complete",
        haveData: true,
        data: {
          transactionId: "",
          documents: [{ documentId: 1, pdfBlobName: "foo" }],
        },
      } as CombinedState["pipelineState"];

      jest.spyOn(apiGateway, "resolvePdfUrl").mockImplementation((blobName) => {
        if (blobName !== "foo") throw new Error();
        return "baz";
      });

      const nextState = reducer(
        {
          documentsState: existingDocumentsState,
          pipelineState: existingPipelineState,
          tabsState: existingTabsState,
        } as CombinedState,
        {
          type: "OPEN_PDF",
          payload: {
            pdfId: 1,
            tabSafeId: "t1",
            mode: "read",
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        }
      );

      expect(nextState.tabsState).toEqual({
        headers: {
          Authorization: "bar",
          "Correlation-Id": "bar1",
        },
        items: [
          {
            documentId: 1,
            clientLockedState: "unlocked",
            mode: "read",
            pdfBlobName: "foo",
            redactionHighlights: [],
            url: "baz",
            tabSafeId: "t1",
          },
        ],
      });
    });

    it("can open a tab when the pdf details are not known", () => {
      const existingTabsState = {
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [],
      } as CombinedState["tabsState"];

      const existingDocumentsState = {
        status: "succeeded",
        data: [{ documentId: 1 }],
      } as CombinedState["documentsState"];

      const existingPipelineState = {
        status: "incomplete",
      } as CombinedState["pipelineState"];

      const nextState = reducer(
        {
          documentsState: existingDocumentsState,
          pipelineState: existingPipelineState,
          tabsState: existingTabsState,
        } as CombinedState,
        {
          type: "OPEN_PDF",
          payload: {
            pdfId: 1,
            tabSafeId: "t1",
            mode: "read",
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        }
      );

      expect(nextState.tabsState).toEqual({
        headers: {
          Authorization: "bar",
          "Correlation-Id": "bar1",
        } as HeadersInit,
        items: [
          {
            documentId: 1,
            clientLockedState: "unlocked",
            url: undefined,

            redactionHighlights: [],
            mode: "read",
            tabSafeId: "t1",
          },
        ],
      });
    });

    describe("reopening pdfs", () => {
      it("can reopen a read mode pdf and show the previously visible document", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [{ documentId: 1, mode: "read" }],
        } as CombinedState["tabsState"];

        const nextState = reducer(
          {
            documentsState: { status: "succeeded" },
            tabsState: existingTabsState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              pdfId: 1,
              tabSafeId: "t1",
              mode: "read",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          documentsState: { status: "succeeded" },
          searchState: {
            isResultsVisible: false,
          },
          tabsState: {
            ...existingTabsState,
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        });
      });

      it("can reopen a search mode pdf and show the previously visible document", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [{ documentId: 1, mode: "search", searchTerm: "foo" }],
        } as CombinedState["tabsState"];

        const nextState = reducer(
          {
            searchState: { submittedSearchTerm: "foo" },
            documentsState: { status: "succeeded" },
            tabsState: existingTabsState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              pdfId: 1,
              tabSafeId: "t1",
              mode: "search",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          documentsState: { status: "succeeded" },
          searchState: {
            submittedSearchTerm: "foo",
            isResultsVisible: false,
          },
          tabsState: {
            ...existingTabsState,
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        });
      });

      it("can reopen a read mode pdf in search mode", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [
            { documentId: 0, mode: "read" },
            { documentId: 1, mode: "read" },
            { documentId: 2, mode: "read" },
          ],
        } as CombinedState["tabsState"];

        const existingDocumentsState = {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        } as CombinedState["documentsState"];

        const existingSearchState = {
          submittedSearchTerm: "foo",
          results: {
            status: "succeeded",
            data: {
              documentResults: [
                {
                  documentId: 1,
                  occurrences: [
                    {
                      pageIndex: 0,
                      pageHeight: 11.69,
                      pageWidth: 8.27,

                      occurrencesInLine: [[21, 21, 9, 9, 23, 23, 9, 9]],
                    },
                  ] as MappedDocumentResult["occurrences"],
                  occurrencesInDocumentCount: 3,
                },
              ],
            },
          },
        } as CombinedState["searchState"];

        const existingPipelineState = {} as CombinedState["pipelineState"];

        const nextState = reducer(
          {
            searchState: existingSearchState,
            documentsState: existingDocumentsState,
            tabsState: existingTabsState,
            pipelineState: existingPipelineState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              pdfId: 1,
              tabSafeId: "t1",
              mode: "search",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          searchState: {
            submittedSearchTerm: "foo",
            results: {
              status: "succeeded",
              data: {
                documentResults: [
                  {
                    documentId: 1,
                    occurrences: [
                      {
                        pageIndex: 0,
                        pageHeight: 11.69,
                        pageWidth: 8.27,

                        occurrencesInLine: [[21, 21, 9, 9, 23, 23, 9, 9]],
                      },
                    ],
                    occurrencesInDocumentCount: 3,
                  },
                ],
              },
            },
            isResultsVisible: false,
          },
          documentsState: { status: "succeeded", data: [] },
          tabsState: {
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
            items: [
              { documentId: 0, mode: "read" },
              {
                documentId: 1,
                mode: "search",
                clientLockedState: "unlocked",
                tabSafeId: "t1",
                searchTerm: "foo",
                occurrencesInDocumentCount: 3,
                searchHighlights: [
                  {
                    id: "0",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 0,
                      boundingRect: {
                        x1: 20.97,
                        x2: 23.03,
                        y1: 20.97,
                        y2: 23.03,
                        width: 8.27,
                        height: 11.69,
                      },
                      rects: [
                        {
                          x1: 20.97,
                          x2: 23.03,
                          y1: 20.97,
                          y2: 23.03,
                          width: 8.27,
                          height: 11.69,
                        },
                      ],
                    },
                  },
                ],
              },
              { documentId: 2, mode: "read" },
            ],
          },
          pipelineState: {},
        });
      });

      it("can reopen a search mode pdf in read mode", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [
            { documentId: "d0", mode: "read" },
            { documentId: 1, mode: "search" },
            { documentId: 2, mode: "read" },
          ],
        } as CombinedState["tabsState"];

        const existingDocumentsState = {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        } as CombinedState["documentsState"];

        const existingSearchState = {
          submittedSearchTerm: "foo",
          results: {
            status: "succeeded",
          },
        } as CombinedState["searchState"];

        const existingPipelineState = {} as CombinedState["pipelineState"];

        const nextState = reducer(
          {
            searchState: existingSearchState,
            documentsState: existingDocumentsState,
            tabsState: existingTabsState,
            pipelineState: existingPipelineState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              pdfId: 1,
              tabSafeId: "t1",
              mode: "read",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          documentsState: existingDocumentsState,
          searchState: { ...existingSearchState, isResultsVisible: false },
          pipelineState: existingPipelineState,
          tabsState: {
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
            items: [
              { documentId: "d0", mode: "read" },
              {
                documentId: 1,
                clientLockedState: "unlocked",
                mode: "read",
                tabSafeId: "t1",
                url: undefined,
              },
              { documentId: 2, mode: "read" },
            ],
          },
        });
      });

      it("can reopen a search mode pdf in search mode with a different search term", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [
            { documentId: "d0", mode: "read" },
            {
              documentId: 1,
              mode: "search",
              searchTerm: "foo",
              occurrencesInDocumentCount: 1,
              pageOccurrences: [
                {
                  boundingBoxes: [[1, 2, 3]],
                  pageIndex: 0,
                  pageHeight: 11.69,
                  pageWidth: 8.27,
                },
              ],
            },
            { documentId: 2, mode: "read" },
          ],
        } as CombinedState["tabsState"];

        const existingDocumentsState = {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        } as CombinedState["documentsState"];

        const existingSearchState = {
          submittedSearchTerm: "bar",
          results: {
            status: "succeeded",
            data: {
              documentResults: [
                {
                  documentId: 1,
                  occurrences: [
                    {
                      pageIndex: 1,
                      pageHeight: 8,
                      pageWidth: 7,
                      occurrencesInLine: [[1, 1, 9, 9, 2, 2]],
                    },
                    {
                      pageIndex: 2,
                      pageHeight: 9,
                      pageWidth: 8,
                      occurrencesInLine: [[2, 2, 9, 9, 3, 3]],
                    },
                    {
                      pageIndex: 2,
                      pageHeight: 10,
                      pageWidth: 9,
                      occurrencesInLine: [[3, 3, 9, 9, 4, 4]],
                    },
                  ] as MappedDocumentResult["occurrences"],
                  occurrencesInDocumentCount: 4,
                },
              ],
            },
          },
        } as CombinedState["searchState"];

        const existingPipelineState = {} as CombinedState["pipelineState"];

        const nextState = reducer(
          {
            searchState: existingSearchState,
            documentsState: existingDocumentsState,
            tabsState: existingTabsState,
            pipelineState: existingPipelineState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              pdfId: 1,
              tabSafeId: "t1",
              mode: "search",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          searchState: {
            submittedSearchTerm: "bar",
            results: {
              status: "succeeded",
              data: {
                documentResults: [
                  {
                    documentId: 1,
                    occurrences: [
                      {
                        pageIndex: 1,
                        pageHeight: 8,
                        pageWidth: 7,
                        occurrencesInLine: [[1, 1, 9, 9, 2, 2]],
                      },
                      {
                        pageIndex: 2,
                        pageHeight: 9,
                        pageWidth: 8,
                        occurrencesInLine: [[2, 2, 9, 9, 3, 3]],
                      },
                      {
                        pageIndex: 2,
                        pageHeight: 10,
                        pageWidth: 9,
                        occurrencesInLine: [[3, 3, 9, 9, 4, 4]],
                      },
                    ],
                    occurrencesInDocumentCount: 4,
                  },
                ],
              },
            },
            isResultsVisible: false,
          },
          documentsState: { status: "succeeded", data: [] },
          tabsState: {
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
            items: [
              { documentId: "d0", mode: "read" },
              {
                documentId: 1,
                mode: "search",
                searchTerm: "bar",
                occurrencesInDocumentCount: 4,
                pageOccurrences: [
                  {
                    boundingBoxes: [[1, 2, 3]],
                    pageIndex: 0,
                    pageHeight: 11.69,
                    pageWidth: 8.27,
                  },
                ],
                clientLockedState: "unlocked",
                tabSafeId: "t1",
                searchHighlights: [
                  {
                    id: "0",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 1,
                      boundingRect: {
                        x1: 0.97,
                        y1: 0.97,
                        x2: 2.03,
                        y2: 2.03,
                        width: 7,
                        height: 8,
                      },
                      rects: [
                        {
                          x1: 0.97,
                          y1: 0.97,
                          x2: 2.03,
                          y2: 2.03,
                          width: 7,
                          height: 8,
                        },
                      ],
                    },
                  },
                  {
                    id: "1",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 2,
                      boundingRect: {
                        x1: 1.97,
                        y1: 1.97,
                        x2: 3.03,
                        y2: 3.03,
                        width: 8,
                        height: 9,
                      },
                      rects: [
                        {
                          x1: 1.97,
                          y1: 1.97,
                          x2: 3.03,
                          y2: 3.03,
                          width: 8,
                          height: 9,
                        },
                      ],
                    },
                  },
                  {
                    id: "2",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 2,
                      boundingRect: {
                        x1: 2.97,
                        y1: 2.97,
                        x2: 4.03,
                        y2: 4.03,
                        width: 8,
                        height: 9,
                      },
                      rects: [
                        {
                          x1: 2.97,
                          y1: 2.97,
                          x2: 4.03,
                          y2: 4.03,
                          width: 8,
                          height: 9,
                        },
                      ],
                    },
                  },
                ],
              },
              { documentId: 2, mode: "read" },
            ],
          },
          pipelineState: {},
        });
      });
    });
  });

  describe("CLOSE_PDF", () => {
    it("can close a tab", () => {
      const existingTabsState = {
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [
          {
            documentId: 1,
            url: undefined,
            tabSafeId: "t1",
          },
          {
            documentId: 2,
            url: undefined,
            tabSafeId: "t2",
          },
        ],
      } as CombinedState["tabsState"];

      const nextState = reducer(
        {
          tabsState: existingTabsState,
        } as CombinedState,
        { type: "CLOSE_PDF", payload: { tabSafeId: "t2" } }
      );

      expect(nextState.tabsState).toEqual({
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [
          {
            documentId: 1,
            url: undefined,
            tabSafeId: "t1",
          },
        ],
      });
    });
  });

  describe("UPDATE_SEARCH_TERM", () => {
    it("can update search term", () => {
      const existingState = { searchTerm: "foo" } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_TERM",
        payload: { searchTerm: "bar" },
      });

      expect(nextState).toEqual({
        searchTerm: "bar",
      });
    });
  });

  describe("CLOSE_SEARCH_RESULTS", () => {
    it("can close search results", () => {
      const existingSearchState = {
        isResultsVisible: true,
      } as CombinedState["searchState"];

      const nextState = reducer(
        { searchState: existingSearchState } as CombinedState,
        { type: "CLOSE_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        isResultsVisible: false,
      });
    });
  });

  describe("LAUNCH_SEARCH_RESULTS", () => {
    beforeEach(() => {
      jest
        .spyOn(sanitizeSearchTerm, "sanitizeSearchTerm")
        .mockImplementation((input) => {
          if (input === "foo") {
            return "bar";
          }
          throw new Error("Should not be here");
        });
    });

    it("can open search results", () => {
      const existingSearchState = {
        isResultsVisible: false,
      } as CombinedState["searchState"];

      const nextState = reducer(
        {
          searchTerm: "foo",
          searchState: existingSearchState,
        } as CombinedState,
        { type: "LAUNCH_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        submittedSearchTerm: "bar",
        requestedSearchTerm: "foo",
        isResultsVisible: true,
      } as CombinedState["searchState"]);
    });

    it("can trim spaces from the search term", () => {
      const existingSearchState = {
        isResultsVisible: false,
      } as CombinedState["searchState"];

      const nextState = reducer(
        {
          searchTerm: " foo ",
          searchState: existingSearchState,
        } as CombinedState,
        { type: "LAUNCH_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        submittedSearchTerm: "bar",
        requestedSearchTerm: "foo",
        isResultsVisible: true,
      } as CombinedState["searchState"]);
    });
  });

  describe("UPDATE_SEARCH_RESULTS", () => {
    it("throws if search call fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_SEARCH_RESULTS",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
          },
        })
      ).toThrowError(ERROR);
    });

    it("returns a loading searchState if the search call is loading", () => {
      const nextState = reducer({ searchState: {} } as CombinedState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "loading",
        },
      });

      expect(nextState).toEqual({
        searchState: {
          results: {
            status: "loading",
          },
        },
      });
    });

    it("does not alter state if documentState is not succeeded", () => {
      const existingState = {
        documentsState: { status: "loading" },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: [],
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("does not alter state if pipelineState is not succeeded", () => {
      const existingState = {
        documentsState: { status: "succeeded" },
        pipelineState: { status: "incomplete" },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: [],
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("does not alter state if there is no submitted search term", () => {
      const existingState = {
        documentsState: { status: "succeeded" },
        pipelineState: { status: "complete" },
        searchState: { submittedSearchTerm: "" },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: [],
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("does not alter state if the search call carries undefined data", () => {
      const existingState = {
        documentsState: { status: "succeeded" },
        pipelineState: { status: "complete" },
        searchState: { submittedSearchTerm: "foo" },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: undefined,
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("can update search results, update missing documents and build filter options", () => {
      const existingState = {
        documentsState: { status: "succeeded", data: {} },
        pipelineState: { status: "complete", haveData: true, data: {} },
        searchState: { submittedSearchTerm: "foo", resultsOrder: "byDateDesc" },
      } as CombinedState;

      const inputPayload = {
        status: "succeeded",
        data: [],
      } as ApiResult<undefined | ApiTextSearchResult[]>;

      const mockUnsortedData = {} as MappedTextSearchResult;
      const mockData = {} as MappedTextSearchResult;
      const mockMissingDocs = {} as CombinedState["searchState"]["missingDocs"];
      const mockFilterOptions =
        {} as CombinedState["searchState"]["filterOptions"];

      jest
        .spyOn(textSearchMapper, "mapTextSearch")
        .mockImplementation((textSearchResult, mappedCaseDocuments) => {
          if (
            inputPayload.status === "succeeded" &&
            textSearchResult === inputPayload.data &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockUnsortedData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, resultOrder) => {
          if (
            mappedTextSearchResult === mockUnsortedData &&
            resultOrder === existingState.searchState.resultsOrder
          ) {
            return mockData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(missingDocuments, "mapMissingDocuments")
        .mockImplementation((pipelineResults, mappedCaseDocuments) => {
          if (
            pipelineResults ===
              (existingState.pipelineState.haveData &&
                existingState.pipelineState.data) &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockMissingDocs;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(filters, "mapFilters")
        .mockImplementation((mappedTextSearchResult) => {
          if (mappedTextSearchResult === mockUnsortedData) {
            return mockFilterOptions;
          }
          throw new Error("Unexpected mock function arguments");
        });

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: inputPayload,
      });

      expect(nextState).toEqual({
        ...existingState,
        searchState: {
          ...existingState.searchState,
          missingDocs: mockMissingDocs,
          filterOptions: mockFilterOptions,
          results: {
            status: "succeeded",
            data: mockData,
          },
        },
      });

      expect(nextState.searchState.missingDocs).toBe(mockMissingDocs);
      expect(nextState.searchState.filterOptions).toBe(mockFilterOptions);

      expect(
        nextState.searchState.results.status === "succeeded" &&
          nextState.searchState.results.data
      ).toBe(mockData);
    });
  });

  describe("CHANGE_RESULTS_ORDER", () => {
    it("can update the stored results order but not change search results ordering if the search state is still loading", () => {
      const existingState = {
        searchState: {
          results: { status: "loading" },
          resultsOrder: "byOccurancesPerDocumentDesc",
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "CHANGE_RESULTS_ORDER",
        payload: "byDateDesc",
      });

      expect(nextState).toEqual({
        searchState: {
          results: {
            status: "loading",
          },
          resultsOrder: "byDateDesc",
        },
      });
    });

    it("can update results ordering if search state is ready", () => {
      const existingMappedTextSearchResult = {} as MappedTextSearchResult;
      const expectedMappedTextSearchResult = {} as MappedTextSearchResult;

      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, sortOrder) => {
          if (mappedTextSearchResult !== existingMappedTextSearchResult)
            throw new Error();
          if (sortOrder !== "byDateDesc") throw new Error();
          return expectedMappedTextSearchResult;
        });

      const existingState = {
        searchState: {
          results: {
            status: "succeeded",
            data: existingMappedTextSearchResult,
          },
          resultsOrder: "byOccurancesPerDocumentDesc",
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "CHANGE_RESULTS_ORDER",
        payload: "byDateDesc",
      });

      expect(nextState).toEqual({
        searchState: {
          results: {
            status: "succeeded",
            data: expectedMappedTextSearchResult,
          },
          resultsOrder: "byDateDesc",
        },
      });
    });
  });

  describe("UPDATE_FILTER", () => {
    it("can update filters but not sort data if search state is still loading", () => {
      const existingSearchState = {
        results: { status: "loading" },

        filterOptions: {
          category: {
            a: { label: "", count: -1, isSelected: true },
            b: { label: "", count: -1, isSelected: false },
          },
          docType: {
            a: { label: "", count: -1, isSelected: true },
            b: { label: "", count: -1, isSelected: true },
          },
        } as CombinedState["searchState"]["filterOptions"],
      } as CombinedState["searchState"];

      const result = reducer(
        { searchState: existingSearchState } as CombinedState,
        {
          type: "UPDATE_FILTER",
          payload: { filter: "docType", id: "b", isSelected: true },
        }
      );

      expect(result).toEqual({
        searchState: {
          filterOptions: {
            category: {
              a: {
                count: -1,
                isSelected: true,
                label: "",
              },
              b: {
                count: -1,
                isSelected: false,
                label: "",
              },
            },
            docType: {
              a: {
                count: -1,
                isSelected: true,
                label: "",
              },
              b: {
                count: -1,
                isSelected: true,
                label: "",
              },
            },
          },
          results: {
            status: "loading",
          },
        },
      });
    });

    it("can update filters and sort data if search state has succeeded", () => {
      jest
        .spyOn(documentVisibility, "isDocumentVisible")
        .mockImplementation(({ documentId }, filterOptions) => {
          switch (documentId) {
            case 1:
              return { isVisible: true, hasChanged: true };
            case 2:
              return { isVisible: false, hasChanged: true };
            case 3:
              return { isVisible: true, hasChanged: false };
            default:
              throw new Error("Unexpected mock function arguments");
          }
        });

      const existingSearchState = {
        results: {
          status: "succeeded",
          data: {
            documentResults: [
              { documentId: 1, occurrencesInDocumentCount: 2 },
              { documentId: 2, occurrencesInDocumentCount: 3 },
              { documentId: 3, occurrencesInDocumentCount: 7 },
            ] as MappedDocumentResult[],
          },
        },

        filterOptions: {
          category: {},
          docType: {},
        } as CombinedState["searchState"]["filterOptions"],
      } as CombinedState["searchState"];

      const result = reducer(
        { searchState: existingSearchState } as CombinedState,
        {
          type: "UPDATE_FILTER",
          payload: { filter: "docType", id: "a", isSelected: true },
        }
      );

      expect(result).toEqual({
        searchState: {
          filterOptions: {
            category: {},
            docType: {
              a: { isSelected: true },
            },
          },
          results: {
            data: {
              documentResults: [
                {
                  documentId: 1,
                  isVisible: true,
                  occurrencesInDocumentCount: 2,
                },
                {
                  documentId: 2,
                  isVisible: false,
                  occurrencesInDocumentCount: 3,
                },
                { documentId: 3, occurrencesInDocumentCount: 7 },
              ],
              filteredDocumentCount: 1,
              filteredOccurrencesCount: 2,
            },
            status: "succeeded",
          },
        },
      });

      // assert we have been given the same reference to the object if
      //  the document has not changed
      expect(
        result.searchState.results.status === "succeeded" &&
          result.searchState.results.data.documentResults[2]
      ).toBe(
        existingSearchState.results.status === "succeeded" &&
          existingSearchState.results.data.documentResults[2]
      );
    });
  });

  describe("ADD_REDACTION", () => {
    it("can add a redaction", () => {
      jest.useFakeTimers().setSystemTime(new Date("2022-01-01"));

      const existingTabsState = {
        items: [
          {
            documentId: 1,
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 2 },
                id: "1640995199999",
              },
            ] as IPdfHighlight[],
          },
          { documentId: "bar", redactionHighlights: [] as IPdfHighlight[] },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "ADD_REDACTION",
          payload: {
            pdfId: 1,
            redaction: {
              type: "redaction",
              position: { pageNumber: 1 },
            } as NewPdfHighlight,
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            {
              documentId: 1,
              redactionHighlights: [
                {
                  type: "redaction",
                  position: { pageNumber: 2 },
                  id: "1640995199999",
                },
                {
                  type: "redaction",
                  position: { pageNumber: 1 },
                  id: "1640995200000",
                },
              ],
            },
            { documentId: "bar", redactionHighlights: [] },
          ],
        },
      });
    });
  });

  describe("REMOVE_REDACTION", () => {
    it("can remove a redaction", () => {
      jest.useFakeTimers().setSystemTime(new Date("2022-01-01"));

      const existingTabsState = {
        items: [
          {
            documentId: 1,
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 2 },
                id: "1640995199999",
              },
              {
                type: "redaction",
                position: { pageNumber: 1 },
                id: "1640995200000",
              },
            ] as IPdfHighlight[],
          },
          { documentId: "bar", redactionHighlights: [] as IPdfHighlight[] },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "REMOVE_REDACTION",
          payload: {
            pdfId: 1,
            redactionId: "1640995200000",
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            {
              documentId: 1,
              redactionHighlights: [
                {
                  type: "redaction",
                  position: { pageNumber: 2 },
                  id: "1640995199999",
                },
              ],
            },
            { documentId: "bar", redactionHighlights: [] },
          ],
        },
      });
    });
  });

  describe("REMOVE_ALL_REDACTIONS", () => {
    it("can remove all redactions", () => {
      jest.useFakeTimers().setSystemTime(new Date("2022-01-01"));

      const existingTabsState = {
        items: [
          {
            documentId: 1,
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 2 },
                id: "1640995199999",
              },
              {
                type: "redaction",
                position: { pageNumber: 1 },
                id: "1640995200000",
              },
            ] as IPdfHighlight[],
          },
          {
            documentId: 2,
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 3 },
                id: "1640995199998",
              },
            ] as IPdfHighlight[],
          },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "REMOVE_ALL_REDACTIONS",
          payload: {
            pdfId: 1,
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            {
              documentId: 1,
              redactionHighlights: [],
            },
            {
              documentId: 2,
              redactionHighlights: [
                {
                  type: "redaction",
                  position: { pageNumber: 3 },
                  id: "1640995199998",
                },
              ],
            },
          ],
        },
      });
    });
  });

  describe("UPDATE_DOCUMENT_LOCK_STATE", () => {
    it("can update document locked state", () => {
      const existingTabsState = {
        items: [
          {
            documentId: 1,
            clientLockedState: "unlocked",
          },
          {
            documentId: 2,
            clientLockedState: "unlocking",
          },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: {
            pdfId: 1,
            lockedState: "locked",
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            { documentId: 1, clientLockedState: "locked" },
            { documentId: 2, clientLockedState: "unlocking" },
          ],
        },
      });
    });
  });

  describe("UPDATE_SAVED_STATE", () => {
    it("can update ssaved state", () => {});
  });

  it("can not handle an unknown action", () => {
    expect(() =>
      reducer({} as CombinedState, { type: "UNKNOWN" } as any)
    ).toThrow();
  });
});
