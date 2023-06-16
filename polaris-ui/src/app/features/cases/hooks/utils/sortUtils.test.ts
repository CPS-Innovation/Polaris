import {
  sortDocumentsByCreatedDate,
  sortAscendingByDocumentTypeAndCreationDate,
  sortAscendingByListOrderAndId,
  customSortByDocumentType,
} from "./sortUtils";
import { PresentationDocumentProperties } from "../../domain/gateway/PipelineDocument";

describe("sortUtils", () => {
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
      const sortedList = unsortedList.sort(customSortByDocumentType);
      expect(sortedList).toEqual([
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
    it("Should successfully do the custom sort without breaking even if some values are null or undefined", () => {
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
      const sortedList = unsortedList.sort(customSortByDocumentType);
      expect(sortedList).toEqual([
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
    it("Should successfully do the custom sort without breaking even if some values are null or undefined", () => {
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
      const sortedList = unsortedList.sort(customSortByDocumentType);
      expect(sortedList).toEqual([
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
  });
});
