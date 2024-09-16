import {
  handleTextValidation,
  EXHIBIT_TEXT_VALIDATION_REGEX,
  EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX,
} from "./textValidation";

describe("textValidation", () => {
  describe("EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX", () => {
    test("should validate correctly the `EXHIBIT_TEXT_VALIDATION_REGEX`", () => {
      const inputString = `abc1!@£$%^&*()_+={}[]:;"\<,>?/~.-|21A`;
      const result = handleTextValidation(
        inputString,
        EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX
      );
      expect(result).toEqual('invalid characters 1!@£$%^&*()_+={}[]:;"<,>?/~2');
    });

    test("should only report the unique invalid characters", () => {
      const inputString = `abc11++++12=A`;
      const result = handleTextValidation(
        inputString,
        EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX
      );
      expect(result).toEqual("invalid characters 1+2=");
    });

    test("should show the text wording correctly if there is only one unique invalid character", () => {
      const inputString = `abc1111111A`;
      const result = handleTextValidation(
        inputString,
        EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX
      );
      expect(result).toEqual("invalid character 1");
    });

    test("should return empty string if there are no invalid characters in the string", () => {
      const inputString = `abcA`;
      const result = handleTextValidation(
        inputString,
        EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX
      );
      expect(result).toEqual("");
    });
  });

  describe("EXHIBIT_TEXT_VALIDATION_REGEX ", () => {
    test("should validate correctly the `EXHIBIT_TEXT_VALIDATION_REGEX`", () => {
      const inputString = `abc1!@£$%^&*()_+={}[]:;"\<,>?/~.-|21A`;
      const result = handleTextValidation(
        inputString,
        EXHIBIT_TEXT_VALIDATION_REGEX
      );
      expect(result).toEqual("invalid characters ^={}[]<>");
    });
    test("should return empty string if there are no invalid characters in the string", () => {
      const inputString = `abcA`;
      const result = handleTextValidation(
        inputString,
        EXHIBIT_TEXT_VALIDATION_REGEX
      );
      expect(result).toEqual("");
    });
  });
});
