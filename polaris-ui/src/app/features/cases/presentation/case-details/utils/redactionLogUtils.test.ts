import {
  redactString,
  getPresentationRedactionTypeNames,
  removeNonDigits,
  getDefaultValuesFromMappings,
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

  describe("removeNonDigits", () => {
    test("Should replace all the non digit characters from a string", () => {
      expect(removeNonDigits("CMS-123")).toEqual("123");
      expect(removeNonDigits("CMS-123abc")).toEqual("123");
      expect(removeNonDigits("CMS-   123abc")).toEqual("123");
      expect(removeNonDigits("abc456abc123abc")).toEqual("456123");
    });
  });

  describe("getDefaultValuesFromMappings", () => {
    const mappingData = {
      businessUnits: [
        { ou: "Northern CJU (Bristol)", areaId: "10", unitId: null },
        { ou: "Bristol CC", areaId: "10", unitId: "2" },
        { ou: "Guildford Mags", areaId: "9", unitId: "1" },
      ],
      documentTypes: [
        { cmsDocTypeId: "1201", docTypeId: "37" },
        { cmsDocTypeId: "1029", docTypeId: "35" },
        { cmsDocTypeId: "6", docTypeId: "31" },
      ],
      investigatingAgencies: [{ ouCode: "00AH", investigatingAgencyId: "10" }],
    };
    const ouCodeMapping = [
      {
        ouCode: "45",
        areaCode: "9",
        areaName: "South East",
        investigatingAgencyCode: "61",
        investigatingAgencyName: "Surrey",
      },
    ];
    it("Should return correct default values when doctypeId is 1024", () => {
      const docTypeId = 1029;
      const owningUnit = "Guildford Mags";
      const caseUrn = "45CV2911222";
      const defaultValues = getDefaultValuesFromMappings(
        mappingData,
        ouCodeMapping,
        owningUnit,
        docTypeId,
        caseUrn
      );
      expect(defaultValues).toEqual({
        businessUnit: "1",
        cpsArea: "9",
        documentType: "",
        investigatingAgency: "61",
      });
    });
    it("Should return correct default values ", () => {
      const docTypeId = 6;
      const owningUnit = "Northern CJU (Bristol)";
      const caseUrn = "00AH2911222";
      const defaultValues = getDefaultValuesFromMappings(
        mappingData,
        ouCodeMapping,
        owningUnit,
        docTypeId,
        caseUrn
      );
      expect(defaultValues).toEqual({
        businessUnit: "",
        cpsArea: "10",
        documentType: "31",
        investigatingAgency: "10",
      });
    });
  });
});
