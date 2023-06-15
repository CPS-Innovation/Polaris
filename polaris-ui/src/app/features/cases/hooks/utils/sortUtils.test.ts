import {
  sortDocumentsByCreatedDate,
  sortAscendingByDocumentTypeAndCreationDate,
  sortAscendingByListOrderAndId,
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
          cmsDocType: { documentType: "Abc" },
        },
      ];
      const expectedSortedDocs = [
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          cmsDocType: { documentType: "Abc" },
        },
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          cmsDocType: { documentType: "MG 16" },
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
      ];
      const sortedDocs = sortAscendingByDocumentTypeAndCreationDate(
        unsortedDocs as PresentationDocumentProperties[]
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });
  });
  describe("sortAscendingByListOrderAndId", () => {
    it("Should sort documents in the ascending order of listOrder and then based on documentId", () => {
      const unsortedDocs = [
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: 1,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: 1,
          cmsDocType: { documentType: "MG 3" },
        },

        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          listOrder: 3,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: 4,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: 2,
          cmsDocType: { documentType: "Abc" },
        },
      ];
      const expectedSortedDocs = [
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: 1,
          cmsDocType: { documentType: "MG 3" },
        },
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: 1,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: 2,
          cmsDocType: { documentType: "Abc" },
        },

        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          listOrder: 3,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: 4,
          cmsDocType: { documentType: "MG 7" },
        },
      ];
      const sortedDocs = sortAscendingByListOrderAndId(
        unsortedDocs as PresentationDocumentProperties[]
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });

    it("Should handle even if the listOrder is null and sort by documentId", () => {
      const unsortedDocs = [
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: null,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: null,
          cmsDocType: { documentType: "MG 3" },
        },

        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          listOrder: null,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: null,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: null,
          cmsDocType: { documentType: "Abc" },
        },
      ];
      const expectedSortedDocs = [
        {
          documentId: "1",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: null,
          cmsDocType: { documentType: "MG 3" },
        },
        {
          documentId: "2",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: null,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "3",
          cmsFileCreatedDate: "2020-01-15",
          listOrder: null,
          cmsDocType: { documentType: "MG 7" },
        },
        {
          documentId: "4",
          cmsFileCreatedDate: "2020-01-10",
          listOrder: null,
          cmsDocType: { documentType: "MG 16" },
        },
        {
          documentId: "5",
          cmsFileCreatedDate: "2020-01-01",
          listOrder: null,
          cmsDocType: { documentType: "Abc" },
        },
      ];
      const sortedDocs = sortAscendingByListOrderAndId(
        unsortedDocs as PresentationDocumentProperties[]
      );
      expect(sortedDocs).toEqual(expectedSortedDocs);
    });
  });
});
