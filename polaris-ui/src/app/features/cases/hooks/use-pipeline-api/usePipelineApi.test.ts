import { usePipelineApi } from "./usePipelineApi";
import * as polling from "./initiate-and-poll";
import { PipelineResults } from "../../domain/PipelineResults";
import { renderHook } from "@testing-library/react-hooks";
import { AsyncPipelineResult } from "./AsyncPipelineResult";

describe("usePipelineApi", () => {
  it("can return results", async () => {
    const expectedResults = {} as AsyncPipelineResult<PipelineResults>;

    jest
      .spyOn(polling, "initiateAndPoll")
      .mockImplementation(
        (urn, caseId, pollingDelay, lastProcessingCompleted, del) => {
          new Promise((resolve) => setTimeout(resolve, 50)).then(() =>
            del(expectedResults)
          );
          return () => {};
        }
      );

    const { result, waitForNextUpdate } = renderHook(() =>
      usePipelineApi("0", 1, {
        refreshData: {
          startRefresh: true,
          savedDocumentDetails: [],
        },
        lastProcessingCompleted: "",
      })
    );

    expect(result.current).toEqual({ status: "initiating", haveData: false });

    await waitForNextUpdate();

    expect(result.current).toEqual(expectedResults);
  });
});
