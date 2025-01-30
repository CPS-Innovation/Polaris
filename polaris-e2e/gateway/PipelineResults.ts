import { PipelineDocument } from "./PipelineDocument";
import { InProgressPipelineStatus } from "./PipelineStatus";

export type PipelineResults = {
  documents: PipelineDocument[];
  processingCompleted: string;
  //documentsRetrieved: string;
  status: InProgressPipelineStatus;
};
