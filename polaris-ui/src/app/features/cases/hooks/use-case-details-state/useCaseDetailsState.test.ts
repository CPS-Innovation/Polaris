import { useCaseDetailsState, initialState } from "./useCaseDetailsState";
import * as searchCaseWhenReady from "./search-case-when-ready";
import * as api from "../../api/gateway-api";
import * as pipelineApi from "../use-pipeline-api/usePipelineApi";

import { ApiTextSearchResult } from "../../domain/ApiTextSearchResult";
import { PipelineResults } from "../../domain/PipelineResults";
import { AsyncPipelineResult } from "../use-pipeline-api/AsyncPipelineResult";
import { renderHook } from "@testing-library/react-hooks";
import * as useApi from "../../../../common/hooks/useApi";
import * as reducer from "./reducer";
import { act } from "react-dom/test-utils";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { reducerAsyncActionHandlers } from "./reducer-async-action-handlers";
import { CaseDetails } from "../../domain/CaseDetails";

type ReducerParams = Parameters<typeof reducer.reducer>;
let reducerSpy: jest.SpyInstance<ReducerParams[0]>;

const isSameRef = (a: any, b: any) => a === b;

describe("useCaseDetailsState", () => {
  beforeEach(() => {
    const mockGetCaseDetails = jest
      .spyOn(api, "getCaseDetails")
      .mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve({} as CaseDetails), 100)
          )
      );

    const mockSearchCaseWhenAllReady = jest
      .spyOn(searchCaseWhenReady, "searchCaseWhenReady")
      .mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve([] as ApiTextSearchResult[]), 100)
          )
      );

    const mockSearchCase = jest
      .spyOn(api, "searchCase")
      .mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve([] as ApiTextSearchResult[]), 100)
          )
      );

    jest.spyOn(useApi, "useApi").mockImplementation((del, p0, p1, p2, p3) => {
      if (isSameRef(del, mockGetCaseDetails)) {
        return { status: "succeeded", data: "getCaseDetails" };
      }
      // if (isSameRef(del, mockgetCaseDocumentsList)) {
      //   return { status: "succeeded", data: "getCaseDocumentsList" };
      // }
      if (isSameRef(del, mockSearchCase)) {
        return {
          status: "succeeded",
          data: "searchCase",
        };
      }
      if (isSameRef(del, mockSearchCaseWhenAllReady)) {
        return {
          status: "succeeded",
          data: "searchCaseWhenAllReady",
        };
      }
      throw new Error("Should not be here");
    });

    jest
      .spyOn(pipelineApi, "usePipelineApi")
      .mockImplementation(() => ({} as AsyncPipelineResult<PipelineResults>));

    reducerSpy = jest
      .spyOn(reducer, "reducer")
      .mockImplementation((state) => state);
  });

  afterEach(() => jest.restoreAllMocks());

  describe("initialisation", () => {
    it("initialises to the expected state", () => {
      const { result } = renderHook(() => useCaseDetailsState("bar", 1));

      const {
        handleOpenPdf,
        handleClosePdf,
        handleSearchTermChange,
        handleLaunchSearchResults,
        handleCloseSearchResults,
        handleChangeResultsOrder,
        handleUpdateFilter,
        handleAddRedaction,
        handleRemoveRedaction,
        handleRemoveAllRedactions,
        handleSavedRedactions,
        handleOpenPdfInNewTab,
        ...stateProperties
      } = result.current;

      expect(stateProperties).toEqual({
        caseId: 1,
        urn: "bar",
        ...initialState,
      });
    });

    it("can update state according to the api call results", async () => {
      renderHook(() => useCaseDetailsState("bar", 1));

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_CASE_DETAILS",
        payload: { status: "succeeded", data: "getCaseDetails" },
      });

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_PIPELINE",
        payload: {},
      });

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: "searchCaseWhenAllReady",
        },
      });
    });
  });

  describe("synchronous action handlers", () => {
    it("can close a pdf", () => {
      const {
        result: {
          current: { handleClosePdf },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() => handleClosePdf({ tabSafeId: "1" }));

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "CLOSE_PDF",
        payload: { tabSafeId: "1" },
      });
    });

    it("can update search term", () => {
      const {
        result: {
          current: { handleSearchTermChange },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() => handleSearchTermChange("foo"));

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_SEARCH_TERM",
        payload: {
          searchTerm: "foo",
        },
      });
    });

    it("can launch search results", () => {
      const {
        result: {
          current: { handleLaunchSearchResults },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() => handleLaunchSearchResults());

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "LAUNCH_SEARCH_RESULTS",
      });
    });

    it("can close search results", () => {
      const {
        result: {
          current: { handleCloseSearchResults },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() => handleCloseSearchResults());

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "CLOSE_SEARCH_RESULTS",
      });
    });

    it("can change results order", () => {
      const {
        result: {
          current: { handleChangeResultsOrder },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() => handleChangeResultsOrder("byDateDesc"));

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "CHANGE_RESULTS_ORDER",
        payload: "byDateDesc",
      });
    });

    it("can update a filter", () => {
      const {
        result: {
          current: { handleUpdateFilter },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() =>
        handleUpdateFilter({ filter: "category", id: "1", isSelected: true })
      );

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_FILTER",
        payload: { filter: "category", id: "1", isSelected: true },
      });
    });
  });
  describe("async action handlers", () => {
    it("can open a pdf in a new tab", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(reducerAsyncActionHandlers, "REQUEST_OPEN_PDF_IN_NEW_TAB")
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleOpenPdfInNewTab },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() => handleOpenPdfInNewTab(2));

      expect(mockHandler).toBeCalledWith({
        type: "REQUEST_OPEN_PDF_IN_NEW_TAB",
        payload: { pdfId: 2 },
      });
    });

    it("can open a pdf", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(reducerAsyncActionHandlers, "REQUEST_OPEN_PDF")
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleOpenPdf },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      act(() => handleOpenPdf({ tabSafeId: "1", documentId: 2, mode: "read" }));

      expect(mockHandler).toBeCalledWith({
        type: "REQUEST_OPEN_PDF",
        payload: { tabSafeId: "1", pdfId: 2, mode: "read" },
      });
    });

    it("can add a redaction", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(reducerAsyncActionHandlers, "ADD_REDACTION_AND_POTENTIALLY_LOCK")
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleAddRedaction },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      handleAddRedaction(2, { type: "redaction" } as NewPdfHighlight);

      expect(mockHandler).toBeCalledWith({
        type: "ADD_REDACTION_AND_POTENTIALLY_LOCK",
        payload: { pdfId: 2, redaction: { type: "redaction" } },
      });
    });

    it("can remove a redaction", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(
          reducerAsyncActionHandlers,
          "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK"
        )
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleRemoveRedaction },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      handleRemoveRedaction(2, "baz");

      expect(mockHandler).toBeCalledWith({
        type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK",
        payload: { pdfId: 2, redactionId: "baz" },
      });
    });

    it("can remove all redactions", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(reducerAsyncActionHandlers, "REMOVE_ALL_REDACTIONS_AND_UNLOCK")
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleRemoveAllRedactions },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      handleRemoveAllRedactions(2);

      expect(mockHandler).toBeCalledWith({
        type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
        payload: { pdfId: 2 },
      });
    });

    it("can save all redactions", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(reducerAsyncActionHandlers, "SAVE_REDACTIONS")
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleSavedRedactions },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1));

      handleSavedRedactions(2);

      expect(mockHandler).toBeCalledWith({
        type: "SAVE_REDACTIONS",
        payload: { pdfId: 2 },
      });
    });
  });
});
