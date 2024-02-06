import {
  redactString,
  getPresentationRedactionTypeNames,
} from "./redactionLogUtils";

describe("redactionLogUtils", () => {
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
  describe("getPresentationRedactionTypeNames", () => {
    test("Should return the redaction type names correctly, when count is one", () => {
      const redactionTypes = [
        "Named individual",
        "Title",
        "Occupation",
        "Relationship to others",
        "Address",
        "Location",
        "Vehicle registration",
        "NHS number",
        "Date of birth",
        "Bank details",
        "NI Number",
        "Phone number",
        "Email address",
        "Previous convictions",
        "Other",
      ];

      redactionTypes.forEach((type) => {
        if (type === "Previous convictions") {
          expect(getPresentationRedactionTypeNames(1, type)).toEqual(
            "Previous conviction"
          );
          return;
        }
        expect(getPresentationRedactionTypeNames(1, type)).toEqual(type);
      });
    });

    test("Should return the redaction type names correctly, when count is more than one", () => {
      const redactionTypes = [
        "Named individual",
        "Title",
        "Occupation",
        "Relationship to others",
        "Address",
        "Location",
        "Vehicle registration",
        "NHS number",
        "Date of birth",
        "Bank details",
        "NI Number",
        "Phone number",
        "Email address",
        "Previous convictions",
        "Other",
      ];

      const expectedResults = [
        "Named individuals",
        "Titles",
        "Occupations",
        "Relationships to others",
        "Addresses",
        "Locations",
        "Vehicle registrations",
        "NHS numbers",
        "Dates of birth",
        "Bank details",
        "NI Numbers",
        "Phone numbers",
        "Email addresses",
        "Previous convictions",
        "Others",
      ];

      redactionTypes.forEach((type, index) => {
        expect(getPresentationRedactionTypeNames(2, type)).toEqual(
          expectedResults[index]
        );
      });
    });
  });
});
