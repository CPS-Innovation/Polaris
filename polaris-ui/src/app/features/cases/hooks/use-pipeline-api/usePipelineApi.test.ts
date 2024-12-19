import { usePipelineApi } from "./usePipelineApi";
import * as polling from "./initiate-and-poll";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { renderHook } from "@testing-library/react-hooks";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import * as shouldTriggerPipelineRefresh from "../utils/shouldTriggerPipelineRefresh";

describe("usePipelineApi", () => {
  it("should trigger pipeline refresh and dispatch UPDATE_PIPELINE with correct payload", async () => {
    const expectedPipelineResults = {
      status: "incomplete",
      data: {},
      correlationId: "corr_id",
    } as AsyncPipelineResult<PipelineResults>;

    jest
      .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
      .mockImplementation((a, b) => {
        return true;
      });

    jest
      .spyOn(polling, "initiateAndPoll")
      .mockImplementation(
        (
          urn,
          caseId,
          pollingDelay,
          lastProcessingCompleted,
          correlationId,
          del,
          isMounting
        ) => {
          new Promise((resolve) => setTimeout(resolve, 50)).then(() =>
            del(expectedPipelineResults)
          );
          return () => {};
        }
      );
    const mockDispatch = jest.fn();
    const { waitForNextUpdate } = renderHook(() =>
      usePipelineApi(
        "0",
        1,
        {
          startPipelineRefresh: true,
          lastProcessingCompleted: "",
          localLastRefreshTime: "",
        },
        "",
        () => false,
        mockDispatch
      )
    );

    expect(mockDispatch).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_PIPELINE",
      payload: { correlationId: "", status: "initiating" },
    });

    await waitForNextUpdate();
    expect(mockDispatch).toHaveBeenCalledTimes(2);

    expect(mockDispatch).toHaveBeenNthCalledWith(2, {
      type: "UPDATE_PIPELINE",
      payload: expectedPipelineResults,
    });
    expect(polling.initiateAndPoll).toHaveBeenCalledTimes(1);
  });

  it("should not trigger pipeline refresh and should not dispatch UPDATE_PIPELINE if shouldTriggerPipelineRefresh is false ", async () => {
    const expectedPipelineResults = {
      status: "incomplete",
      data: {},
      correlationId: "corr_id",
    } as AsyncPipelineResult<PipelineResults>;

    jest
      .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
      .mockImplementation((a, b) => {
        return false;
      });

    jest
      .spyOn(polling, "initiateAndPoll")
      .mockImplementation(
        (
          urn,
          caseId,
          pollingDelay,
          lastProcessingCompleted,
          correlationId,
          del,
          isMounting
        ) => {
          new Promise((resolve) => setTimeout(resolve, 50)).then(() =>
            del(expectedPipelineResults)
          );
          return () => {};
        }
      );
    const mockDispatch = jest.fn();
    renderHook(() =>
      usePipelineApi(
        "0",
        1,
        {
          startPipelineRefresh: true,
          lastProcessingCompleted: "",
          localLastRefreshTime: "",
        },
        "",
        () => false,
        mockDispatch
      )
    );
    expect(polling.initiateAndPoll).toHaveBeenCalledTimes(0);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
  });

  it("should not trigger pipeline refresh and should not dispatch UPDATE_PIPELINE if startPipelineRefresh is false ", async () => {
    const expectedPipelineResults = {
      status: "incomplete",
      data: {},
      correlationId: "corr_id",
    } as AsyncPipelineResult<PipelineResults>;

    jest
      .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
      .mockImplementation((a, b) => {
        return true;
      });

    jest
      .spyOn(polling, "initiateAndPoll")
      .mockImplementation(
        (
          urn,
          caseId,
          pollingDelay,
          lastProcessingCompleted,
          correlationId,
          del,
          isMounting
        ) => {
          new Promise((resolve) => setTimeout(resolve, 50)).then(() =>
            del(expectedPipelineResults)
          );
          return () => {};
        }
      );
    const mockDispatch = jest.fn();
    renderHook(() =>
      usePipelineApi(
        "0",
        1,
        {
          startPipelineRefresh: false,
          lastProcessingCompleted: "",
          localLastRefreshTime: "",
        },
        "",
        () => false,
        mockDispatch
      )
    );
    expect(polling.initiateAndPoll).toHaveBeenCalledTimes(0);
    expect(mockDispatch).toHaveBeenCalledTimes(0);
  });
});
