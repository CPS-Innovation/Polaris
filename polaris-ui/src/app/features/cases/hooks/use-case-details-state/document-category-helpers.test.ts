import {
  sortDocumentsByCreatedDate,
  sortAscendingByDocumentTypeAndCreationDate,
  sortAscendingByListOrderAndId,
  customSortByDocumentType,
  isUnusedCommunicationMaterial,
} from "./document-category-helpers";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

describe("DocumentCategory Helpers", () => {
  describe("sortDocumentsByCreatedDate", () => {
    it("Should sort documents in the ascending order, based on cmsFileCreatedDate", () => {
      const unsortedDocs = [
        { documentId: "1", cmsFileCreatedDate: "2020-01-10" },
        { documentId: "4", cmsFileCreatedDate: "2020-01-10" },
        { documentId: "3", cmsFileCreatedDate: "2020-01-15" },
        { documentId: "2", cmsFileCreatedDate: "2020-01-01" },
      ];
      const expectedSortedDocs = [
        { documentId: "2", cmsFileCreatedDate: "2020-01-01" },
        { documentId: "1", cmsFileCreatedDate: "2020-01-10" },
        { documentId: "4", cmsFileCreatedDate: "2020-01-10" },
        { documentId: "3", cmsFileCreatedDate: "2020-01-15" },
      ];
      const sortedDocs = sortDocumentsByCreatedDate(
        unsortedDocs as PresentationDocumentProperties[]
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });
    it("Should sort documents in the descending order, based on cmsFileCreatedDate", () => {
      const unsortedDocs = [
        { documentId: "1", cmsFileCreatedDate: "2020-01-10" },
        { documentId: "3", cmsFileCreatedDate: "2020-01-15" },
        { documentId: "2", cmsFileCreatedDate: "2020-01-01" },
      ];
      const expectedSortedDocs = [
        { documentId: "3", cmsFileCreatedDate: "2020-01-15" },
        { documentId: "1", cmsFileCreatedDate: "2020-01-10" },
        { documentId: "2", cmsFileCreatedDate: "2020-01-01" },
      ];
      const sortedDocs = sortDocumentsByCreatedDate(
        unsortedDocs as PresentationDocumentProperties[],
        "descending"
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });
  });
  describe("sortAscendingByDocumentTypeAndCreationDate", () => {
    it("Should sort documents in the ascending order of documentType and then based on cmsFileCreatedDate", () => {
      const unsortedDocs = [
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          cmsDocType: { documentType: "MG 3" },
        },
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          cmsDocType: { documentType: "MG2" },
        },
      ];
      const expectedSortedDocs = [
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          cmsDocType: { documentType: "MG2" },
        },
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          cmsDocType: { documentType: "MG 3" },
        },

        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          cmsDocType: { documentType: "MG 16" },
        },
      ];
      const sortedDocs = sortAscendingByDocumentTypeAndCreationDate(
        unsortedDocs as PresentationDocumentProperties[]
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });
  });
  describe("sortAscendingByListOrderAndId", () => {
    it("Should sort documents in the ascending order of categoryListOrder and then based on documentId", () => {
      const unsortedDocs = [
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: 1,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: 1,
          cmsDocType: { documentType: "MG 3" },
        },

        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          categoryListOrder: 3,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: 4,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: 2,
          cmsDocType: { documentType: "Abc" },
        },
      ];
      const expectedSortedDocs = [
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: 1,
          cmsDocType: { documentType: "MG 3" },
        },
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: 1,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: 2,
          cmsDocType: { documentType: "Abc" },
        },

        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          categoryListOrder: 3,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: 4,
          cmsDocType: { documentType: "MG 7" },
        },
      ];
      const sortedDocs = sortAscendingByListOrderAndId(
        unsortedDocs as PresentationDocumentProperties[]
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });

    it("Should handle even if the categoryListOrder is null and sort by documentId", () => {
      const unsortedDocs = [
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: null,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: 1,
          cmsDocType: { documentType: "MG 3" },
        },

        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          categoryListOrder: null,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: 2,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: null,
          cmsDocType: { documentType: "Abc" },
        },
      ];
      const expectedSortedDocs = [
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: 1,
          cmsDocType: { documentType: "MG 3" },
        },
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: 2,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          categoryListOrder: null,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          categoryListOrder: null,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          categoryListOrder: null,
          cmsDocType: { documentType: "Abc" },
        },
      ];
      const sortedDocs = sortAscendingByListOrderAndId(
        unsortedDocs as PresentationDocumentProperties[]
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });
  });
  describe("customSortByDocumentType", () => {
    it("Should successfully do the custom sort by splitting the doctype into three parts prefix, mid and postfix", () => {
      const unsortedList = [
        "MG 12",
        "MG6",
        "MG 6",
        "MG 6A",
        "MG 6B",
        "MG 6C",
        "MG7A",
        "MG 7",
      ];
      unsortedList.sort(customSortByDocumentType);
      expect(unsortedList).toEqual([
        "MG6",
        "MG 6",
        "MG 6A",
        "MG 6B",
        "MG 6C",
        "MG 7",
        "MG7A",
        "MG 12",
      ]);
    });
    it("Should successfully do the custom sort without breaking, even if some values are null or undefined", () => {
      const unsortedList = [
        null,
        "MG(6)",
        "MG6",
        null,
        "MG 6C",
        "MG7D",
        "MG 7A",
        "ABC",
      ] as any;
      unsortedList.sort(customSortByDocumentType);
      expect(unsortedList).toEqual([
        "ABC",
        "MG(6)",
        "MG6",
        "MG 6C",
        "MG 7A",
        "MG7D",
        null,
        null,
      ]);
    });
    it("Should successfully do the custom sort without breaking, even if some values are null or undefined", () => {
      const unsortedList = [
        null,
        null,
        "MG 6",
        null,
        "MG 3",
        "MG 6C",
        "MG7A",
        "MG",
        null,
      ] as any;
      unsortedList.sort(customSortByDocumentType);
      expect(unsortedList).toEqual([
        "MG",
        "MG 3",
        "MG 6",
        "MG 6C",
        "MG7A",
        null,
        null,
        null,
        null,
      ]);
    });

    it("Should successfully do the custom sort and only consider the first 12 characters", () => {
      const unsortedList = [
        "MG 6ABCDEFGHZ",
        "MG 6ABCDEFGHM",
        "MG 6ABCDEFGAD",
      ] as any;
      unsortedList.sort(customSortByDocumentType);
      expect(unsortedList).toEqual([
        "MG 6ABCDEFGAD",
        "MG 6ABCDEFGHZ",
        "MG 6ABCDEFGHM",
      ]);
    });
  });
  describe("isUnusedCommunicationMaterial", () => {
    it("Should return false, if the documentTypeId is not 1029", () => {
      const result = isUnusedCommunicationMaterial("UN CM01", 1030);
      expect(result).toEqual(false);
    });

    it("Should return false, if the presentationTitle is not empty", () => {
      expect(isUnusedCommunicationMaterial(" ", 1030)).toEqual(false);
    });

    it("Should return false, if the presentationTitle contains `UM/`", () => {
      expect(isUnusedCommunicationMaterial("abcUM/def", 1030)).toEqual(false);
    });

    it("Should return true, if the presentationTitle contains `UM` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcUMdef", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc UM /def", 1029)).toEqual(true);
      expect(isUnusedCommunicationMaterial("UM abcr /def", 1029)).toEqual(true);
      expect(isUnusedCommunicationMaterial("abcr /def UM", 1029)).toEqual(true);
    });

    it("Should return true, if the presentationTitle contains `UM+digit` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcUM12def", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc UM12 /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("UM34 abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def UM54", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `UNUSED` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcUNUSEDdef", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc UNUSED /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("UNUSED abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def UNUSED", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `UNUSED+digit` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcUNUSED12def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc UNUSED11 /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("UNUSED34 abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def UNUSED33", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `-UM` as a suffix to a word", () => {
      expect(isUnusedCommunicationMaterial("abcdef -UM bcd", 1029)).toEqual(
        false
      );
      expect(
        isUnusedCommunicationMaterial("abcdef ad-UMabd bcd", 1029)
      ).toEqual(false);
      expect(isUnusedCommunicationMaterial("abcdef 23-UM bcd", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abc-UM dbab def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcm abcr-UM", 1029)).toEqual(true);
      expect(
        isUnusedCommunicationMaterial("abcr /def-UM UNUSED33", 1029)
      ).toEqual(true);
    });

    it("Should return true, if the presentationTitle contains `-UM+digit` as a suffix to a word", () => {
      expect(isUnusedCommunicationMaterial("abcdef -UM233 bcd", 1029)).toEqual(
        false
      );
      expect(
        isUnusedCommunicationMaterial("abcdef -UM233ee bcd", 1029)
      ).toEqual(false);
      expect(
        isUnusedCommunicationMaterial("abcdef 123-UM12r bcd", 1029)
      ).toEqual(false);
      expect(
        isUnusedCommunicationMaterial("abcdef 12-UM123 bcd", 1029)
      ).toEqual(true);
      expect(isUnusedCommunicationMaterial("abc-UM12 dbab def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcm abcr-UM133", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `MG6C` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcMG6Cdef", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc MG6C /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abc -MG6C /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc ?MG6C /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("MG6C abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def MG6C", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `MG6D` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcMG6Ddef", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc MG6D /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abc -MG6D /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc ?MG6D /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("MG6D abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def MG6D", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `MG6E` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcMG6Edef", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc MG6E /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abc -MG6E /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc ?MG6E /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("MG6E abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def MG6E", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `MG06C` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcMG06Cef", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc MG06C /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abc -MG06C /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc ?MG06C /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("MG06C abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def MG06C", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `MG06D` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcMG06Def", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc MG06D /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abc -MG06D /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc ?MG06D /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("MG06D abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def MG06D", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `MG06E` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcMG06Eef", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc MG06E /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abc -MG06E /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc ?MG06E /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("MG06E abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def MG06E", 1029)).toEqual(
        true
      );
    });

    it("Should return true, if the presentationTitle contains `SDC` as standalone word", () => {
      expect(isUnusedCommunicationMaterial("abcSDCef", 1029)).toEqual(false);
      expect(isUnusedCommunicationMaterial("abc SDC /def", 1029)).toEqual(true);
      expect(isUnusedCommunicationMaterial("abc -SDC /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("abc ?SDC /def", 1029)).toEqual(
        false
      );
      expect(isUnusedCommunicationMaterial("SDC abcr /def", 1029)).toEqual(
        true
      );
      expect(isUnusedCommunicationMaterial("abcr /def SDC", 1029)).toEqual(
        true
      );
    });
  });
});
