import {
  InProgressPipelineStatus,
  getPipelineCompletionStatus,
  isDocumentsPresentStatus,
} from "./PipelineStatus";

describe("PipelineStatus", () => {
  describe("getPipelineCompletionStatus", () => {
    it("returns the expected SummaryPipelineStatus for each InProgressPipelineStatus", () => {
      expect(getPipelineCompletionStatus("NotStarted")).toBe("NotCompleted");
      expect(getPipelineCompletionStatus("Running")).toBe("NotCompleted");
      expect(getPipelineCompletionStatus("DocumentsRetrieved")).toBe(
        "NotCompleted"
      );
      expect(getPipelineCompletionStatus("Completed")).toBe("Completed");
      expect(getPipelineCompletionStatus("Failed")).toBe("Failed");
    });
  });

  describe("isDocumentsPresentStatus", () => {
    it.each([
      ["NotStarted", false],
      ["Running", false],
      ["DocumentsRetrieved", true],
      ["Completed", true],
      ["Failed", true],
    ])("returns expected result for %s", (status, expected) => {
      expect(isDocumentsPresentStatus(status as InProgressPipelineStatus)).toBe(
        expected
      );
    });
  });
});
