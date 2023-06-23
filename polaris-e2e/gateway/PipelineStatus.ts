const InProgressPipelineStatusesArray = [
  "NotStarted",
  "Running",
  "DocumentsRetrieved",
  "Completed",
  "Failed",
] as const;

const SummaryPipelineStatusesArray = [
  "NotCompleted",
  "Completed",
  "Failed",
] as const;

const pipelineSucceededStatuses: InProgressPipelineStatus[] = ["Completed"];

const pipelineFailedStatuses: InProgressPipelineStatus[] = ["Failed"];

const pipelineDocumentsPresentStatuses: InProgressPipelineStatus[] = [
  "DocumentsRetrieved",
  "Completed",
  "Failed",
];

export const getPipelineCompletionStatus = (
  status: InProgressPipelineStatus
): SummaryPipelineStatus => {
  if (pipelineSucceededStatuses.includes(status)) {
    return "Completed";
  }
  if (pipelineFailedStatuses.includes(status)) {
    return "Failed";
  }
  return "NotCompleted";
};

export const isDocumentsPresentStatus = (status: InProgressPipelineStatus) =>
  pipelineDocumentsPresentStatuses.includes(status);

type PipelineStatusesTuple = typeof InProgressPipelineStatusesArray;

export type InProgressPipelineStatus = PipelineStatusesTuple[number];

type SummaryPipelineStatusesTuple = typeof SummaryPipelineStatusesArray;

export type SummaryPipelineStatus = SummaryPipelineStatusesTuple[number];
