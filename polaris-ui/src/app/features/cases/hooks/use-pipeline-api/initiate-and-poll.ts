import { ApiError } from "../../../../common/errors/ApiError";
import { AsyncPipelineResult } from "./AsyncPipelineResult";
import { getPipelinePdfResults, initiatePipeline } from "../../api/gateway-api";
import { PipelineResults } from "../../domain/PipelineResults";
import { getPipelinpipelineCompletionStatus } from "../../domain/PipelineStatus";
import { CombinedState } from "../../domain/CombinedState";

const delay = (delayMs: number) =>
  new Promise((resolve) => setTimeout(resolve, delayMs));

const isNewTime = (currentTime: string, lastTime: string) => {
  if (currentTime && !lastTime) {
    return true;
  }
  if (new Date(currentTime) > new Date(lastTime)) {
    return true;
  }
  return false;
};
const hasAnyDocumentUpdated = (
  savedDocumentDetails: {
    documentId: string;
    polarisDocumentVersionId: number;
  }[],
  pipelineResult: PipelineResults
) => {
  if (!savedDocumentDetails.length) {
    return true;
  }
  return savedDocumentDetails.some((document) =>
    hasDocumentUpdated(document, pipelineResult)
  );
};

const hasDocumentUpdated = (
  document: { documentId: string; polarisDocumentVersionId: number },
  newData: PipelineResults
) => {
  const savedDocument = newData.documents.find(
    (newDocument) => newDocument.documentId === document.documentId
  );
  if (!savedDocument) {
    return false;
  }
  if (
    savedDocument.polarisDocumentVersionId ===
    document.polarisDocumentVersionId + 1
  ) {
    return true;
  }
  return false;
};

export const initiateAndPoll = (
  // todo: _ wrap up in to an object arg
  urn: string,
  caseId: number,
  delayMs: number,
  generalPipelineState: CombinedState["generalPipelineState"],

  del: (pipelineResults: AsyncPipelineResult<PipelineResults>) => void
) => {
  let keepPolling = true;
  let trackingCallCount = 0;

  const {
    lastProcessingCompleted,
    refreshData: { savedDocumentDetails },
  } = generalPipelineState;

  const handleApiCallSuccess = (pipelineResult: PipelineResults) => {
    trackingCallCount += 1;

    const completionStatus = getPipelinpipelineCompletionStatus(
      pipelineResult.status
    );
    if (
      completionStatus === "Completed" &&
      isNewTime(pipelineResult.processingCompleted, lastProcessingCompleted) &&
      hasAnyDocumentUpdated(savedDocumentDetails, pipelineResult)
    ) {
      del({
        status: "complete",
        data: pipelineResult,
        haveData: true,
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
    });
  };

  const startInitiatePipelinePolling = async () => {
    while (keepPolling) {
      try {
        await delay(delayMs);
        const trackerArgs = await initiatePipeline(urn, caseId);
        if (trackerArgs.status !== 423) {
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

        const pipelineResult = await getPipelinePdfResults(
          trackerArgs.trackerUrl,
          trackerArgs.correlationId
        );

        handleApiCallSuccess(pipelineResult);
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
