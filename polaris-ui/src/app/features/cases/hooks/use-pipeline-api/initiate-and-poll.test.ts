import { ApiResult } from "../../../../common/types/ApiResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import * as api from "../../api/gateway-api";
import { waitFor } from "@testing-library/react";
import { ApiError } from "../../../../common/errors/ApiError";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import sinon from "sinon";

const POLLING_INTERVAL_MS = 175;

const clock = sinon.useFakeTimers();
describe("initiateAndPoll", () => {
  beforeAll(() => {});
  it("can return failed and stop polling if initiate errors", async () => {
    const expectedError = new ApiError("", "", { status: 100, statusText: "" });
    const initiatePipelineSpy = jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) => Promise.reject(expectedError));

    const mockCallback = jest.fn();
    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      {
        startPipelineRefresh: false,
        lastProcessingCompleted: "",
      },
      "corId_1",
      mockCallback,
      () => false
    );
    clock.tick(1000);
    await waitFor(() => initiatePipelineSpy.mock.calls.length > 0);
    await waitFor(() => expect(initiatePipelineSpy).toHaveBeenCalledTimes(1));
    await waitFor(() => mockCallback.mock.calls.length > 0);
    await waitFor(() =>
      expect(mockCallback).toHaveBeenCalledWith({
        status: "failed",
        error: expectedError,
        httpStatusCode: 100,
        haveData: false,
        correlationId: "corId_1",
      } as AsyncPipelineResult<PipelineResults>)
    );

    quitFn();
  });

  it("can return failed and stop polling if getPipelinePdfResults errors", async () => {
    const initiatePipelineSpy = jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) =>
        Promise.resolve({
          trackerUrl: "foo",
          correlationId: "bar",
          status: 200,
        })
      );

    const expectedError = new ApiError("", "", { status: 100, statusText: "" });
    const getPipelinePdfResultsSpy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation(() => Promise.reject(expectedError));

    const mockCallback = jest.fn();

    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      {
        startPipelineRefresh: false,
        lastProcessingCompleted: "",
      },
      "corId_1",
      mockCallback,
      () => false
    );
    clock.tick(1000);
    await waitFor(() => initiatePipelineSpy.mock.calls.length > 0);
    await waitFor(() => expect(initiatePipelineSpy).toHaveBeenCalledTimes(1));
    clock.tick(1000);
    await waitFor(() => getPipelinePdfResultsSpy.mock.calls.length > 0);
    await waitFor(() =>
      expect(getPipelinePdfResultsSpy).toHaveBeenCalledTimes(1)
    );
    await waitFor(() =>
      expect(mockCallback).toHaveBeenCalledWith({
        status: "failed",
        error: expectedError,
        httpStatusCode: 100,
        haveData: false,
        correlationId: "corId_1",
      } as ApiResult<PipelineResults>)
    );

    quitFn();
  });

  it("can return failed and stop polling if getPipelinePdfResults returns failed", async () => {
    const initiatePipelineSpy = jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) =>
        Promise.resolve({
          trackerUrl: "foo",
          correlationId: "bar",
          status: 200,
        })
      );

    const getPipelinePdfResultsSpy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation((caseId) =>
        Promise.resolve({
          status: "Failed",
          documents: [{ status: "PdfUploadedToBlob" }],
        } as PipelineResults)
      );

    const mockCallback = jest.fn();
    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      {
        startPipelineRefresh: false,

        lastProcessingCompleted: "",
      },
      "corId_1",
      mockCallback,
      () => false
    );
    clock.tick(1000);
    await waitFor(() => initiatePipelineSpy.mock.calls.length > 0);
    await waitFor(() => expect(initiatePipelineSpy).toHaveBeenCalledTimes(1));
    clock.tick(1000);
    await waitFor(() => mockCallback.mock.calls.length > 0);
    await waitFor(() => expect(mockCallback).toHaveBeenCalledTimes(1));
    await waitFor(() => getPipelinePdfResultsSpy.mock.calls.length > 0);
    await waitFor(() =>
      expect(getPipelinePdfResultsSpy).toHaveBeenCalledTimes(1)
    );
    await waitFor(() =>
      expect(mockCallback).toHaveBeenCalledWith({
        status: "failed",
        error: expect.any(Error),
        httpStatusCode: undefined,
        haveData: false,
        correlationId: "corId_1",
      } as ApiResult<PipelineResults>)
    );

    quitFn();
  });

  it("can return an immediately available result", async () => {
    const initiatePipelineSpy = jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) => {
        return Promise.resolve({
          trackerUrl: "foo",
          correlationId: "bar",
          status: 200,
        });
      });
    const mockCallback = jest.fn();
    const expectedResults = {
      status: "Completed",
      processingCompleted: new Date().toISOString(),
      documents: [{}],
    } as PipelineResults;

    const getPipelinePdfResultsSpy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation(() => Promise.resolve(expectedResults));

    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      {
        startPipelineRefresh: false,

        lastProcessingCompleted: "",
      },
      "corId_1",
      mockCallback,
      () => false
    );

    clock.tick(1000);
    await waitFor(() => initiatePipelineSpy.mock.calls.length > 0);
    await waitFor(() => expect(initiatePipelineSpy).toHaveBeenCalledTimes(1));

    clock.tick(1000);
    await waitFor(() => getPipelinePdfResultsSpy.mock.calls.length > 0);
    await waitFor(() =>
      expect(getPipelinePdfResultsSpy).toHaveBeenCalledTimes(1)
    );

    await waitFor(() => mockCallback.mock.calls.length > 0);
    await waitFor(() => expect(mockCallback).toHaveBeenCalledTimes(1));

    await waitFor(() =>
      expect(mockCallback).toHaveBeenCalledWith({
        status: "complete",
        haveData: true,
        correlationId: "corId_1",
        data: expectedResults,
      })
    );
    quitFn();
  });

  it("can poll to retrieve a result", async () => {
    const initiatePipelineSpy = jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) =>
        Promise.resolve({
          trackerUrl: "foo",
          correlationId: "bar",
          status: 200,
        })
      );

    const expectedInterimResults = {
      status: "Running",
      processingCompleted: "",
      documents: [{ status: "PdfUploadedToBlob" }, { status: "None" }],
    } as PipelineResults;

    const expectedFinalResults = {
      status: "Completed",
      processingCompleted: new Date().toISOString(),
      documents: [
        { status: "PdfUploadedToBlob" },
        { status: "PdfUploadedToBlob" },
      ],
    } as PipelineResults;

    let runIndex = 0;
    const getPipelinePdfResultsSpy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation(() => {
        if (runIndex === 0) {
          runIndex += 1;
          return Promise.resolve(expectedInterimResults);
        } else {
          return Promise.resolve(expectedFinalResults);
        }
      });
    const mockCallback = jest.fn();
    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      {
        startPipelineRefresh: false,

        lastProcessingCompleted: "",
      },
      "corId_1",
      mockCallback,
      () => false
    );
    clock.tick(1000);
    await waitFor(() => initiatePipelineSpy.mock.calls.length > 0);
    await waitFor(() => expect(initiatePipelineSpy).toHaveBeenCalledTimes(1));
    clock.tick(1000);
    await waitFor(() => getPipelinePdfResultsSpy.mock.calls.length > 0);
    await waitFor(() =>
      expect(getPipelinePdfResultsSpy).toHaveBeenCalledTimes(1)
    );
    await waitFor(() => mockCallback.mock.calls.length > 0);
    await waitFor(() => expect(mockCallback).toHaveBeenCalledTimes(1));

    await waitFor(() =>
      expect(mockCallback).toHaveBeenNthCalledWith(1, {
        status: "incomplete",
        haveData: true,
        correlationId: "corId_1",
        data: expectedInterimResults,
      })
    );
    clock.tick(1000);
    await waitFor(() => mockCallback.mock.calls.length > 1);
    await waitFor(() => expect(mockCallback).toHaveBeenCalledTimes(2));
    await waitFor(() =>
      expect(mockCallback).toHaveBeenNthCalledWith(2, {
        status: "complete",
        haveData: true,
        correlationId: "corId_1",
        data: expectedFinalResults,
      })
    );
    quitFn();
  });
});
