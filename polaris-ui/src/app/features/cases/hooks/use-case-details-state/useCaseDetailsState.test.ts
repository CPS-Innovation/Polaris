import { useCaseDetailsState } from "./useCaseDetailsState";
import * as api from "../../api/gateway-api";
import * as pipelineApi from "../use-pipeline-api/usePipelineApi";

import { ApiTextSearchResult } from "../../domain/gateway/ApiTextSearchResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { AsyncPipelineResult } from "../use-pipeline-api/AsyncPipelineResult";
import { renderHook, act } from "@testing-library/react";
import * as useApi from "../../../../common/hooks/useApi";
import * as reducer from "./reducer";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { reducerAsyncActionHandlers } from "./reducer-async-action-handlers";
import { CaseDetails } from "../../domain/gateway/CaseDetails";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
} from "../../domain/redactionLog/RedactionLogData";
import { MemoryRouter } from "react-router-dom";
import { initialState } from "../../domain/CombinedState";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

jest.mock("../../../../common/hooks/useAppInsightsTracks", () => ({
  useAppInsightsTrackEvent: () => jest.fn(),
}));

jest.mock(".../../../../auth/msal/useUserGroupsFeatureFlag", () => ({
  useUserGroupsFeatureFlag: () => jest.fn(),
}));

jest.mock("../../../../auth", () => ({ useUserDetails: () => jest.fn() }));

type ReducerParams = Parameters<typeof reducer.reducer>;
let reducerSpy: jest.SpyInstance<ReducerParams[0]>;

const isSameRef = (a: any, b: any) => a === b;

describe("useCaseDetailsState", () => {
  const isUnMounting = () => false;
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
    const getDocumentsList = jest
      .spyOn(api, "getDocumentsList")
      .mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(
              () => resolve([] as PresentationDocumentProperties[]),
              100
            )
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
      if (isSameRef(del, getDocumentsList)) {
        return {
          status: "succeeded",
          data: "getDocumentList",
        };
      }
      throw new Error("Should not be here");
    });

    jest.spyOn(pipelineApi, "usePipelineApi").mockImplementation(
      () =>
        ({ pipelineBusy: false, pipelineResults: {} } as {
          pipelineResults: AsyncPipelineResult<PipelineResults>;
          pipelineBusy: boolean;
        })
    );

    reducerSpy = jest
      .spyOn(reducer, "reducer")
      .mockImplementation((state) => state);
  });

  afterEach(() => jest.restoreAllMocks());

  describe("initialisation", () => {
    it("initialises to the expected state", () => {
      const { result } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

      const {
        handleOpenPdf,
        handleClosePdf,
        handleSearchTermChange,
        handleSearchTypeChange,
        handleLaunchSearchResults,
        handleCloseSearchResults,
        handleChangeResultsOrder,
        handleUpdateFilter,
        handleAddRedaction,
        handleRemoveRedaction,
        handleRemoveAllRedactions,
        handleSavedRedactions,
        handleTabSelection,
        handleCloseErrorModal,
        handleUnLockDocuments,
        handleShowHideDocumentIssueModal,
        handleShowRedactionLogModal,
        handleSaveRedactionLog,
        handleHideRedactionLogModal,
        handleAreaOnlyRedaction,
        handleSaveReadUnreadData,
        handleAddNote,
        handleGetNotes,
        handleShowHideRedactionSuggestions,
        handleSearchPIIAction,
        handleResetRenameData,
        handleSaveRename,
        handleReclassifySuccess,
        handleResetReclassifyData,
        handleAddPageRotation,
        handleRemovePageRotation,
        handleShowHidePageRotation,
        handleRemoveAllRotations,
        handleSaveRotations,
        handleClearAllNotifications,
        handleClearNotification,
        handleUpdateConversionStatus,
        handleShowHidePageDeletion,
        handleHideSaveRotationModal,
        handleAccordionOpenClose,
        handleAccordionOpenCloseAll,
        handleToggleDocumentState,
        handleUpdateDCFAction,
        ...stateProperties
      } = result.current;

      expect(stateProperties).toEqual({
        combinedState: {
          caseId: 1,
          urn: "bar",
          ...initialState,
        },
      });
    });

    it("can update state according to the api call results", async () => {
      renderHook(() => useCaseDetailsState("bar", 1, undefined, isUnMounting), {
        wrapper: MemoryRouter,
      });

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_CASE_DETAILS",
        payload: { status: "succeeded", data: "getCaseDetails" },
      });
      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_CASE_DETAILS",
        payload: { status: "succeeded", data: "getCaseDetails" },
      });

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "UPDATE_DOCUMENTS",
        payload: { status: "succeeded", data: "getDocumentList" },
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
      renderHook(() => useCaseDetailsState("bar", 1, undefined, isUnMounting), {
        wrapper: MemoryRouter,
      });

      expect(reducerSpy).not.toBeCalledWith(expect.anything(), {
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
      renderHook(() => useCaseDetailsState("bar", 1, undefined, isUnMounting), {
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

  //Note:Make sure all the handlers are added in the test case
  describe("synchronous action handlers", () => {
    it("can close a pdf", () => {
      const {
        result: {
          current: { handleClosePdf },
        },
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

      act(() => handleClosePdf("1"));

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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );
      act(() => handleCloseErrorModal());

      expect(reducerSpy).toBeCalledWith(expect.anything(), {
        type: "HIDE_ERROR_MODAL",
      });
    });
  });
  describe("async action handlers", () => {
    it("can open a pdf", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(reducerAsyncActionHandlers, "REQUEST_OPEN_PDF")
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleOpenPdf },
        },
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

      act(() => handleOpenPdf({ documentId: "2", mode: "read" }));

      expect(mockHandler).toBeCalledWith({
        type: "REQUEST_OPEN_PDF",
        payload: { documentId: "2", mode: "read" },
      });
    });

    it("can add a redaction", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(
          reducerAsyncActionHandlers,
          "ADD_REDACTION_OR_ROTATION_AND_POTENTIALLY_LOCK"
        )
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleAddRedaction },
        },
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

      handleAddRedaction("2", [{ type: "redaction" }] as NewPdfHighlight[]);

      expect(mockHandler).toBeCalledWith({
        type: "ADD_REDACTION_OR_ROTATION_AND_POTENTIALLY_LOCK",
        payload: { documentId: "2", redactions: [{ type: "redaction" }] },
      });
    });

    it("can remove a redaction", () => {
      const mockHandler = jest.fn();

      jest
        .spyOn(
          reducerAsyncActionHandlers,
          "REMOVE_REDACTION_OR_ROTATION_AND_POTENTIALLY_UNLOCK"
        )
        .mockImplementation(() => mockHandler);

      const {
        result: {
          current: { handleRemoveRedaction },
        },
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

      handleRemoveRedaction("2", "baz");

      expect(mockHandler).toBeCalledWith({
        type: "REMOVE_REDACTION_OR_ROTATION_AND_POTENTIALLY_UNLOCK",
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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

      handleRemoveAllRedactions("2");

      expect(mockHandler).toBeCalledWith({
        type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
        payload: { documentId: "2", type: "redaction" },
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
      } = renderHook(
        () => useCaseDetailsState("bar", 1, undefined, isUnMounting),
        {
          wrapper: MemoryRouter,
        }
      );

      handleSavedRedactions("2");

      expect(mockHandler).toBeCalledWith({
        type: "SAVE_REDACTIONS",
        payload: { documentId: "2", searchPIIOn: false },
      });
    });
  });
});
