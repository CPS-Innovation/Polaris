const InProgressPipelineStatusesArray = [
  "NotStarted",
  "Running",
  "NoDocumentsFoundInCDE",
  "Completed",
  "Failed",
] as const;

const SummaryPipelineStatusesArray = [
  "NotCompleted",
  "Completed",
  "Failed",
] as const;

const pipelineSucceededStatuses: InProgressPipelineStatus[] = [
  "NoDocumentsFoundInCDE",
  "Completed",
];

const pipelineFailedStatuses: InProgressPipelineStatus[] = ["Failed"];

export const getPipelinpipelineCompletionStatus = (
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

type PipelineStatusesTuple = typeof InProgressPipelineStatusesArray;

export type InProgressPipelineStatus = PipelineStatusesTuple[number];

type SummaryPipelineStatusesTuple = typeof SummaryPipelineStatusesArray;

export type SummaryPipelineStatus = SummaryPipelineStatusesTuple[number];
