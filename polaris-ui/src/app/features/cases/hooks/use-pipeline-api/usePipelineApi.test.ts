import { usePipelineApi } from "./usePipelineApi";
import * as polling from "./initiate-and-poll";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { renderHook } from "@testing-library/react-hooks";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import * as shouldTriggerPipelineRefresh from "../utils/shouldTriggerPipelineRefresh";

describe("usePipelineApi", () => {
  it("can return results", async () => {
    const expectedResults = {
      pipelineBusy: true,
      pipelineResults: {} as AsyncPipelineResult<PipelineResults>,
    };

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
          del
        ) => {
          new Promise((resolve) => setTimeout(resolve, 50)).then(() =>
            del(expectedResults.pipelineResults)
          );
          return () => {};
        }
      );
    const mockDispatch = jest.fn();
    const { result, waitForNextUpdate } = renderHook(() =>
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

    expect(mockDispatch).toHaveBeenCalledTimes(2);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_PIPELINE",
      payload: { correlationId: "", status: "initiating" },
    });

    expect(mockDispatch).toHaveBeenNthCalledWith(2, {
      type: "UPDATE_PIPELINE",
      payload: { correlationId: "", status: "initiating" },
    });

    // expect(mockDispatch).nthCalledWith(1)
    //   .expect(result.current)
    //   .toEqual({
    //     pipelineBusy: true,
    //     pipelineResults: {
    //       status: "initiating",
    //       haveData: false,
    //       correlationId: "",
    //     },
    //   });

    // await waitForNextUpdate();

    // expect(result.current).toEqual(expectedResults);
  });
});
