import { isNewTime } from "./refreshUtils";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";
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
});
