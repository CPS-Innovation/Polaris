import { ApiResult } from "../../../../common/types/ApiResult";
import { PipelineResults } from "../../domain/PipelineResults";
import { initiateAndPoll } from "./initiate-and-poll";
import * as api from "../../api/gateway-api";
import { waitFor } from "@testing-library/react";
import { ApiError } from "../../../../common/errors/ApiError";
import { AsyncPipelineResult } from "./AsyncPipelineResult";

const POLLING_INTERVAL_MS = 175;

const ensureHasStoppedPollingHelper = async (
  quitFn: () => void,
  spy: jest.SpyInstance
) => {
  quitFn();
  const callsMadeSoFar = spy.mock.calls.length;
  await new Promise((resolve) => setTimeout(resolve, POLLING_INTERVAL_MS * 5));
  expect(spy.mock.calls.length).toEqual(callsMadeSoFar);
};

describe("initiateAndPoll", () => {
  it("can return failed and stop polling if initiate errors", async () => {
    const expectedError = new ApiError("", "", { status: 100, statusText: "" });
    const spy = jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) => Promise.reject(expectedError));

    let results: AsyncPipelineResult<PipelineResults>;
    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      "",
      (res) => (results = res)
    );

    await waitFor(() =>
      expect(results).toEqual({
        status: "failed",
        error: expectedError,
        httpStatusCode: 100,
        haveData: false,
      } as AsyncPipelineResult<PipelineResults>)
    );

    ensureHasStoppedPollingHelper(quitFn, spy);
  });

  it("can return failed and stop polling if getPipelinePdfResults errors", async () => {
    jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) =>
        Promise.resolve({ trackerUrl: "foo", correlationId: "bar" })
      );

    const expectedError = new ApiError("", "", { status: 100, statusText: "" });
    const spy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation(() => Promise.reject(expectedError));

    let results: AsyncPipelineResult<PipelineResults>;

    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      "",
      (res) => (results = res)
    );

    await waitFor(() =>
      expect(results).toEqual({
        status: "failed",
        error: expectedError,
        httpStatusCode: 100,
        haveData: false,
      } as ApiResult<PipelineResults>)
    );

    ensureHasStoppedPollingHelper(quitFn, spy);
  });

  it("can return failed and stop polling if getPipelinePdfResults returns failed", async () => {
    jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) =>
        Promise.resolve({ trackerUrl: "foo", correlationId: "bar" })
      );

    const spy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation((caseId) =>
        Promise.resolve({
          transactionId: "",
          status: "Failed",
          documents: [{ pdfBlobName: "foo", status: "PdfUploadedToBlob" }],
        } as PipelineResults)
      );

    let results: AsyncPipelineResult<PipelineResults>;

    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      "",
      (res) => (results = res)
    );

    await waitFor(() =>
      expect(results).toEqual({
        status: "failed",
        error: expect.any(Error),
        httpStatusCode: undefined,
        haveData: false,
      } as ApiResult<PipelineResults>)
    );

    ensureHasStoppedPollingHelper(quitFn, spy);
  });

  it("can return an immediately available result", async () => {
    jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) =>
        Promise.resolve({ trackerUrl: "foo", correlationId: "bar" })
      );

    const expectedResults = {
      transactionId: "",
      status: "Completed",
      processingCompleted: new Date().toISOString(),
      documents: [{ pdfBlobName: "foo" }],
    } as PipelineResults;

    const spy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation(() => Promise.resolve(expectedResults));

    let results: AsyncPipelineResult<PipelineResults>;
    const quitFn = initiateAndPoll(
      "0",
      1,
      POLLING_INTERVAL_MS,
      "",
      (res) => (results = res)
    );

    await waitFor(() =>
      expect(results).toEqual({
        status: "complete",
        haveData: true,
        data: expectedResults,
      })
    );

    ensureHasStoppedPollingHelper(quitFn, spy);
  });

  it("can poll to retrieve a result", async () => {
    jest
      .spyOn(api, "initiatePipeline")
      .mockImplementation((caseId) =>
        Promise.resolve({ trackerUrl: "foo", correlationId: "bar" })
      );

    const expectedInterimResults = {
      transactionId: "",
      status: "Running",
      documents: [
        { pdfBlobName: "foo", status: "PdfUploadedToBlob" },
        { status: "None" },
      ],
    } as PipelineResults;

    const expectedFinalResults = {
      transactionId: "",
      status: "Completed",
      processingCompleted: new Date().toISOString(),
      documents: [
        { pdfBlobName: "foo", status: "PdfUploadedToBlob" },
        { pdfBlobName: "bar", status: "PdfUploadedToBlob" },
      ],
    } as PipelineResults;

    let runIndex = 0;
    const spy = jest
      .spyOn(api, "getPipelinePdfResults")
      .mockImplementation(() => {
        if (runIndex === 0) {
          runIndex += 1;
          return Promise.resolve(expectedInterimResults);
        } else {
          return Promise.resolve(expectedFinalResults);
        }
      });

    let results: AsyncPipelineResult<PipelineResults>;
    const quitFn = initiateAndPoll("0", 1, POLLING_INTERVAL_MS, "", (res) => {
      results = res;
    });

    await waitFor(() =>
      expect(results).toEqual({
        status: "incomplete",
        haveData: true,
        data: expectedInterimResults,
      })
    );
    await waitFor(() =>
      expect(results).toEqual({
        status: "complete",
        haveData: true,
        data: expectedFinalResults,
      })
    );

    ensureHasStoppedPollingHelper(quitFn, spy);
  });
});
