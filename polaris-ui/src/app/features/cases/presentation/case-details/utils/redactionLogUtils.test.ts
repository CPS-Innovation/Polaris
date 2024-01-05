import { redactString } from "./redactionLogUtils";

describe.only("redactionLogUtils", () => {
  describe("redactString", () => {
    test("Should replace the characters except first and last,  with `*` by default", () => {
      expect(redactString("Sample Test")).toEqual("S*********t");
      expect(redactString("abcde")).toEqual("a***e");
      expect(redactString("ae")).toEqual("ae");
      expect(redactString("a")).toEqual("a");
    });

    test("Should be able to configure the number of visible characters from from and back of string", () => {
      expect(redactString("Sample Test", 2, 3)).toEqual("Sa******est");
      expect(redactString("abcde", 3, 0)).toEqual("abc**");
    });
  });
});
