import { ApiError } from "../../../../common/errors/ApiError";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import {
  getDocuments,
  getPipelinePdfResults,
  initiatePipeline,
} from "../../api/gateway-api";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { getPipelineCompletionStatus } from "../../domain/gateway/PipelineStatus";
import { CombinedState } from "../../domain/CombinedState";
import {
  isNewTime,
  hasDocumentUpdated,
  LOCKED_STATUS_CODE,
} from "../utils/refreshUtils";
const delay = (delayMs: number) =>
  new Promise((resolve) => setTimeout(resolve, delayMs));

const hasAnyDocumentUpdated = (
  savedDocumentDetails: {
    documentId: string;
    versionId: number;
  }[],
  pipelineResult: PipelineResults
) => {
  return (
    !savedDocumentDetails.length ||
    savedDocumentDetails.some((document) =>
      hasDocumentUpdated(document, pipelineResult.documents)
    )
  );
};

export const initiateAndPoll = (
  urn: string,
  caseId: number,
  delayMs: number,
  pipelineRefreshData: CombinedState["pipelineRefreshData"],
  correlationId: string,
  del: (pipelineResults: AsyncPipelineResult<PipelineResults>) => void,
  shouldStopPolling: () => boolean
) => {
  let keepPolling = true;
  let trackingCallCount = 0;

  const { lastProcessingCompleted, savedDocumentDetails } = pipelineRefreshData;

  const handleApiCallSuccess = (pipelineResult: PipelineResults) => {
    trackingCallCount += 1;

    const completionStatus = getPipelineCompletionStatus(pipelineResult.status);
    if (
      completionStatus === "Completed" &&
      isNewTime(pipelineResult.processingCompleted, lastProcessingCompleted) &&
      // todo: not sure about this
      hasAnyDocumentUpdated(savedDocumentDetails, pipelineResult)
    ) {
      del({
        status: "complete",
        data: pipelineResult,
        haveData: true,
        correlationId,
      });
      keepPolling = false;
    } else if (completionStatus === "Failed") {
      throw new Error(
        `Document processing pipeline returned with "Failed" status after ${trackingCallCount} polling attempts`
      );
    } else {
      del({
        status: "incomplete",
        data: pipelineResult,
        haveData: true,
        correlationId,
      });
    }
  };

  const handleApiCallError = (error: any) => {
    keepPolling = false;
    del({
      status: "failed",
      error,
      httpStatusCode: error instanceof ApiError ? error.code : undefined,
      haveData: false,
      correlationId,
    });
  };

  const startInitiatePipelinePolling = async () => {
    // First we poll the kick-off endpoint until this request is accepted (we poll because
    // we may already have a refresh in-flight.

    // Note: `while (true)` plus using `break` is not enough. We need to be able to cancel
    //  polling from the consumer, hence
    while (keepPolling) {
      try {
        await delay(delayMs);
        if (shouldStopPolling()) {
          keepPolling = false;
          break;
        }
        const trackerArgs = await initiatePipeline(urn, caseId, correlationId);
        // If we get 423 and there are redacted documents, keep polling initiate pipeline
        const shouldKeepPollingInitiate =
          trackerArgs.status === LOCKED_STATUS_CODE &&
          savedDocumentDetails.length; // I'm not sure about this, what about notes?
        if (!shouldKeepPollingInitiate) {
          startTrackerPolling(trackerArgs);
          break;
        }
      } catch (error) {
        handleApiCallError(error);
      }
    }
  };

  const startTrackerPolling = async (
    trackerArgs: Awaited<ReturnType<typeof initiatePipeline>>
  ) => {
    while (keepPolling) {
      try {
        await delay(delayMs);
        if (shouldStopPolling()) {
          keepPolling = false;
          break;
        }

        const [pipelineResults] = await Promise.all([
          getPipelinePdfResults(
            trackerArgs.trackerUrl,
            trackerArgs.correlationId
          ),
          //getDocuments(urn, caseId),
        ]);

        if (pipelineResults) {
          handleApiCallSuccess(pipelineResults);
        }
      } catch (error) {
        handleApiCallError(error);
      }
    }
  };

  const doWork = () => {
    startInitiatePipelinePolling();
  };
  doWork();

  return () => {
    // allow consumer to kill loop
    keepPolling = false;
  };
};
