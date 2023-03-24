import { getPipelinpipelineCompletionStatus } from "./PipelineStatus";

describe("PipelineStatus", () => {
  it("returns the expected SummaryPipelineStatus for each InProgressPipelineStatus", () => {
    expect(getPipelinpipelineCompletionStatus("Running")).toBe("NotCompleted");
    expect(getPipelinpipelineCompletionStatus("DocumentsRetrieved")).toBe(
      "NotCompleted"
    );
    expect(getPipelinpipelineCompletionStatus("Completed")).toBe("Completed");
    expect(getPipelinpipelineCompletionStatus("Failed")).toBe("Failed");
    expect(getPipelinpipelineCompletionStatus("Deleted")).toBe("Failed");
  });
});
