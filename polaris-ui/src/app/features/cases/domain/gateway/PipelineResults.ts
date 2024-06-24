import { PipelineDocument } from "./PipelineDocument";
import { InProgressPipelineStatus } from "./PipelineStatus";

export type PipelineResults = {
  /**
   * @deprecated Not certain that this is used or useful
   */
  transactionId: string;
  documents: PipelineDocument[];
  processingCompleted: string;
  documentsRetrieved: string;
  status: InProgressPipelineStatus;
};
