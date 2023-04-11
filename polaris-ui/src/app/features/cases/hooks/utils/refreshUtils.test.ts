import { isNewTime, hasDocumentUpdated } from "./refreshUtils";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
describe("refreshUtils", () => {
  describe("isNewTime", () => {
    it("it should return true if the lasttime is empty and currentime is non empty", () => {
      const result = isNewTime("2023-04-05T15:02:17.601Z", "");
      expect(result).toEqual(true);
    });
    it("it should return true if the currentime is a newer time compared to lasttime", () => {
      const result = isNewTime(
        "2023-04-05T15:02:18.601Z",
        "2023-04-05T15:02:17.601Z"
      );
      expect(result).toEqual(true);
    });
    it("it should return false if the lasttime is same time compared to currentime", () => {
      const result = isNewTime(
        "2023-04-05T15:02:17.601Z",
        "2023-04-05T15:02:17.601Z"
      );
      expect(result).toEqual(false);
    });
    it("it should return false if the lasttime is older time compared to currentime", () => {
      const result = isNewTime(
        "2023-04-05T15:02:16.601Z",
        "2023-04-05T15:02:17.601Z"
      );
      expect(result).toEqual(false);
    });
    it("it should return false if the lasttime and currentime is empty", () => {
      const result = isNewTime("", "");
      expect(result).toEqual(false);
    });
    it("it shouldn't break if we pass in invalid date and should return false", () => {
      const result = isNewTime("abc", "def");
      expect(result).toEqual(false);
    });
  });

  describe("hasDocumentUpdated", () => {
    it("it should return true if there is matching updated document ", () => {
      const newData = {
        documents: [{ documentId: "1", polarisDocumentVersionId: 2 }],
      } as PipelineResults;
      const result = hasDocumentUpdated(
        { documentId: "1", polarisDocumentVersionId: 1 },
        newData
      );
      expect(result).toEqual(true);
    });
    it("it should return false if there is matching document doesn't have version updated", () => {
      const newData = {
        documents: [{ documentId: "1", polarisDocumentVersionId: 1 }],
      } as PipelineResults;
      const result = hasDocumentUpdated(
        { documentId: "1", polarisDocumentVersionId: 1 },
        newData
      );
      expect(result).toEqual(false);
    });

    it("it should return false if there is no matching document found", () => {
      const newData = {
        documents: [{ documentId: "1", polarisDocumentVersionId: 1 }],
      } as PipelineResults;
      const result = hasDocumentUpdated(
        { documentId: "2", polarisDocumentVersionId: 1 },
        newData
      );
      expect(result).toEqual(false);
    });

    it("it should return false if there is no matching document found", () => {
      const newData = {
        documents: [{ documentId: "1", polarisDocumentVersionId: 1 }],
      } as PipelineResults;
      const result = hasDocumentUpdated(
        { documentId: "1", polarisDocumentVersionId: 3 },
        newData
      );
      expect(result).toEqual(false);
    });
  });
});
