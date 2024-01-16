import { useCaseDetailsState, initialState } from "./useCaseDetailsState";
import * as api from "../../api/gateway-api";
import * as pipelineApi from "../use-pipeline-api/usePipelineApi";

import { ApiTextSearchResult } from "../../domain/gateway/ApiTextSearchResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { AsyncPipelineResult } from "../use-pipeline-api/AsyncPipelineResult";
import { renderHook } from "@testing-library/react-hooks";
import * as useApi from "../../../../common/hooks/useApi";
import * as reducer from "./reducer";
import { act } from "react-dom/test-utils";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { reducerAsyncActionHandlers } from "./reducer-async-action-handlers";
import { CaseDetails } from "../../domain/gateway/CaseDetails";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
} from "../../domain/redactionLog/RedactionLogData";
import { MemoryRouter } from "react-router-dom";

jest.mock("../../../../common/hooks/useAppInsightsTracks", () => ({
  useAppInsightsTrackEvent: () => jest.fn(),
}));

jest.mock(".../../../../auth/msal/useUserGroupsFeatureFlag", () => ({
  useUserGroupsFeatureFlag: () => jest.fn(),
}));

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

    const mockGetRedactionLogLookUpsData = jest
      .spyOn(api, "getRedactionLogLookUpsData")
      .mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve({} as RedactionLogLookUpsData), 100)
          )
      );

    const mockGetRedactionLogMappingData = jest
      .spyOn(api, "getRedactionLogMappingData")
      .mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve({} as RedactionLogMappingData), 100)
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

    jest.spyOn(useApi, "useApi").mockImplementation((del, params) => {
      if (isSameRef(del, mockGetCaseDetails)) {
        return { status: "succeeded", data: "getCaseDetails" };
      }

      if (isSameRef(del, mockGetRedactionLogLookUpsData)) {
        return { status: "succeeded", data: "getRedactionLogLooksUpData" };
      }

      if (isSameRef(del, mockGetRedactionLogMappingData)) {
        return { status: "succeeded", data: "getRedactionLogMappingData" };
      }
      if (isSameRef(del, mockSearchCase)) {
        return {
          status: "succeeded",
          data: "searchCase",
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
      const { result } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

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
        handleTabSelection,
        handleCloseErrorModal,
        handleUnLockDocuments,
        handleShowHideDocumentIssueModal,
        handleShowRedactionLogModal,
        handleSavedRedactionLog,
        ...stateProperties
      } = result.current;

      expect(stateProperties).toEqual({
        caseId: 1,
        urn: "bar",
        ...initialState,
      });
    });

    it("can update state according to the api call results", async () => {
      renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

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
        payload: { status: "succeeded", data: "searchCase" },
      });
    });
    it("should dispatch reducer actions, if the useApi hook return with status 'loading'", () => {
      jest.spyOn(useApi, "useApi").mockImplementation(() => {
        return { status: "loading" };
      });
      renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_CASE_DETAILS",
        payload: { status: "loading" },
      });

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_SEARCH_RESULTS",
        payload: { status: "loading" },
      });
    });

    it("should not dispatch reducer actions, if the useApi hook return with status 'initial'", () => {
      jest.spyOn(useApi, "useApi").mockImplementation(() => {
        return { status: "initial" };
      });
      renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      expect(reducerSpy).not.toBeCalledWith(expect.anything(), {
        type: "UPDATE_CASE_DETAILS",
        payload: { status: "initial" },
      });

      expect(reducerSpy).not.toBeCalledWith(expect.anything(), {
        type: "UPDATE_SEARCH_RESULTS",
        payload: { status: "initial" },
      });
    });
  });

  describe("synchronous action handlers", () => {
    it("can close a pdf", () => {
      const {
        result: {
          current: { handleClosePdf },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      act(() => handleClosePdf({ documentId: "1" }));

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "CLOSE_PDF",
        payload: { pdfId: "1" },
      });
    });

    it("can update search term", () => {
      const {
        result: {
          current: { handleSearchTermChange },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      act(() =>
        handleUpdateFilter({ filter: "category", id: "1", isSelected: true })
      );

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_FILTER",
        payload: { filter: "category", id: "1", isSelected: true },
      });
    });
    it("can hide the error modal", () => {
      const {
        result: {
          current: { handleCloseErrorModal },
        },
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });
      act(() => handleCloseErrorModal());

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "HIDE_ERROR_MODAL",
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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      act(() => handleOpenPdfInNewTab("2"));

      expect(mockHandler).toBeCalledWith({
        type: "REQUEST_OPEN_PDF_IN_NEW_TAB",
        payload: { documentId: "2" },
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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      act(() => handleOpenPdf({ documentId: "2", mode: "read" }));

      expect(mockHandler).toBeCalledWith({
        type: "REQUEST_OPEN_PDF",
        payload: { documentId: "2", mode: "read" },
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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      handleAddRedaction("2", { type: "redaction" } as NewPdfHighlight);

      expect(mockHandler).toBeCalledWith({
        type: "ADD_REDACTION_AND_POTENTIALLY_LOCK",
        payload: { documentId: "2", redaction: { type: "redaction" } },
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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      handleRemoveRedaction("2", "baz");

      expect(mockHandler).toBeCalledWith({
        type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK",
        payload: { documentId: "2", redactionId: "baz" },
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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      handleRemoveAllRedactions("2");

      expect(mockHandler).toBeCalledWith({
        type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
        payload: { documentId: "2" },
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
      } = renderHook(() => useCaseDetailsState("bar", 1), {
        wrapper: MemoryRouter,
      });

      handleSavedRedactions("2");

      expect(mockHandler).toBeCalledWith({
        type: "SAVE_REDACTIONS",
        payload: { documentId: "2" },
      });
    });
  });
});
