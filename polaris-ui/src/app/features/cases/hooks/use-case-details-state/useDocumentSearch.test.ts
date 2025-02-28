import { useDocumentSearch } from "./useDocumentSearch";
import * as useApi from "../../../../common/hooks/useApi";
import { CombinedState } from "../../domain/CombinedState";
import * as shouldTriggerPipelineRefresh from "../utils/shouldTriggerPipelineRefresh";
import { renderHook, act } from "@testing-library/react";

describe("useDocumentSearch", () => {
  it("should fetch new search results and dispatch UPDATE_SEARCH_RESULTS action with correct payload", async () => {
    jest.spyOn(useApi, "useApi").mockImplementation((a, b, shouldMakeCall) => {
      if (!shouldMakeCall) return { status: "initial" };
      return { status: "succeeded", data: [{ id: 1 }] as any };
    });
    jest
      .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
      .mockImplementation((a, b) => {
        return false;
      });

    const mockDispatch = jest.fn();

    const combinedState = {
      searchState: {
        submittedSearchTerm: "abc",
        lastSubmittedSearchTerm: "",
      },
      pipelineState: {
        status: "complete",
      },
      documentsState: {
        status: "succeeded",
      },
    } as CombinedState;

    renderHook(() => useDocumentSearch("0", 1, combinedState, mockDispatch));

    expect(useApi.useApi).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_SEARCH_RESULTS",
      payload: { data: [{ id: 1 }], status: "succeeded" },
    });
  });

  it("should not fetch new documents and dispatch UPDATE_SEARCH_RESULTS action if the triggering of a pipeline refresh is needed", async () => {
    jest.spyOn(useApi, "useApi").mockImplementation((a, b, shouldMakeCall) => {
      if (!shouldMakeCall) return { status: "initial" };
      return { status: "succeeded", data: [{ id: 1 }] as any };
    });
    jest
      .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
      .mockImplementation((a, b) => {
        return true;
      });

    const mockDispatch = jest.fn();

    const combinedState = {
      searchState: {
        submittedSearchTerm: "abc",
        lastSubmittedSearchTerm: "",
      },
      pipelineState: {
        status: "complete",
      },
      documentsState: {
        status: "succeeded",
      },
    } as CombinedState;

    renderHook(() => useDocumentSearch("0", 1, combinedState, mockDispatch));

    expect(useApi.useApi).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
  });

  it("should not fetch new documents and dispatch UPDATE_SEARCH_RESULTS action if there is no submittedSearchTerm, or if the submittedSearchTerm is different from lastSubmittedSearchTerm if there is one, or the pipelineState.status is not 'complete' or documentsState.status is not 'succeeded'", async () => {
    jest.spyOn(useApi, "useApi").mockImplementation((a, b, shouldMakeCall) => {
      if (!shouldMakeCall) return { status: "initial" };
      return { status: "succeeded", data: [{ id: 1 }] as any };
    });
    jest
      .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
      .mockImplementation((a, b) => {
        return false;
      });

    const mockDispatch = jest.fn();

    const combinedState = {
      searchState: {
        submittedSearchTerm: undefined,
        lastSubmittedSearchTerm: "",
      },
      pipelineState: {
        status: "complete",
      },
      documentsState: {
        status: "succeeded",
      },
    } as CombinedState;

    const { rerender } = renderHook(
      ({ combinedState }) =>
        useDocumentSearch("0", 1, combinedState, mockDispatch),
      { initialProps: { combinedState } }
    );

    expect(useApi.useApi).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
    //submitted term same as last submitted term
    rerender({
      combinedState: {
        ...combinedState,
        searchState: {
          ...combinedState.searchState,
          submittedSearchTerm: "abc",
          lastSubmittedSearchTerm: "abc",
        },
      },
    });
    expect(useApi.useApi).toHaveBeenCalledTimes(2);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
    //pipelineState status !=="complete"
    rerender({
      combinedState: {
        ...combinedState,
        searchState: {
          submittedSearchTerm: "abc",
          lastSubmittedSearchTerm: "dec",
        },
        pipelineState: {
          status: "incomplete",
        },
      } as CombinedState,
    });
    expect(useApi.useApi).toHaveBeenCalledTimes(3);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
    //documentsState status !=="succeeded"
    rerender({
      combinedState: {
        ...combinedState,
        searchState: {
          submittedSearchTerm: "abc",
          lastSubmittedSearchTerm: "abc",
        },
        pipelineState: {
          status: "complete",
        },
        documentsState: {
          status: "loading",
        },
      } as unknown as CombinedState,
    });
    expect(useApi.useApi).toHaveBeenCalledTimes(4);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
    rerender({
      combinedState: {
        ...combinedState,
        searchState: {
          submittedSearchTerm: "abc",
          lastSubmittedSearchTerm: "dec",
        },
        pipelineState: {
          status: "complete",
        },
        documentsState: {
          status: "succeeded",
        },
      } as unknown as CombinedState,
    });
    expect(useApi.useApi).toHaveBeenCalledTimes(5);
    expect(mockDispatch).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_SEARCH_RESULTS",
      payload: { data: [{ id: 1 }], status: "succeeded" },
    });
  });
});
