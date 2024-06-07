import { getFilteredByValidUkPhoneNumber } from "./map-searchPII-highlights";
import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";

describe("map-searchPII-highlights", () => {
  describe("getFilteredByValidUkPhoneNumber", () => {
    it("Should accepts all valid 10 digit phone numbers  highlights with piiCategory `PhoneNumber`", () => {
      const highlights = [
        { id: "1", piiCategory: "PhoneNumber", textContent: "+44712345678" },
        { id: "2", piiCategory: "PhoneNumber", textContent: "+44 712345678" },
        { id: "3", piiCategory: "PhoneNumber", textContent: "+44112345678" },
        { id: "4", piiCategory: "PhoneNumber", textContent: "+44 112345678" },
        { id: "5", piiCategory: "PhoneNumber", textContent: "+44212345678" },
        { id: "6", piiCategory: "PhoneNumber", textContent: "+44 212345678" },
        { id: "7", piiCategory: "PhoneNumber", textContent: "+44312345678" },
        { id: "8", piiCategory: "PhoneNumber", textContent: "+44 312345678" },
        { id: "9", piiCategory: "PhoneNumber", textContent: "+44812345678" },
        { id: "10", piiCategory: "PhoneNumber", textContent: "+44 812345678" },
        { id: "11", piiCategory: "PhoneNumber", textContent: "+44912345678" },
        { id: "12", piiCategory: "PhoneNumber", textContent: "+44 912345678" },
        { id: "13", piiCategory: "PhoneNumber", textContent: "0712345678" },
        { id: "14", piiCategory: "PhoneNumber", textContent: "0112345678" },
        { id: "15", piiCategory: "PhoneNumber", textContent: "0212345678" },
        { id: "16", piiCategory: "PhoneNumber", textContent: "0312345678" },
        { id: "17", piiCategory: "PhoneNumber", textContent: "0812345678" },
        { id: "18", piiCategory: "PhoneNumber", textContent: "0912345678" },
        { id: "19", piiCategory: "PhoneNumber", textContent: "(0112)345678" },
        { id: "20", piiCategory: "PhoneNumber", textContent: "(0112) 345678" },
        { id: "21", piiCategory: "PhoneNumber", textContent: "(0112) 345 678" },
        { id: "22", piiCategory: "PhoneNumber", textContent: "(0112)-345-678" },
        { id: "23", piiCategory: "PhoneNumber", textContent: "(0212)345678" },
        { id: "24", piiCategory: "PhoneNumber", textContent: "(0212) 345678" },
        { id: "25", piiCategory: "PhoneNumber", textContent: "(0212) 345 678" },
        { id: "26", piiCategory: "PhoneNumber", textContent: "(0212)-345-678" },
        { id: "27", piiCategory: "PhoneNumber", textContent: "(0312)345678" },
        { id: "28", piiCategory: "PhoneNumber", textContent: "(0312) 345678" },
        { id: "29", piiCategory: "PhoneNumber", textContent: "(0312) 345 678" },
        { id: "30", piiCategory: "PhoneNumber", textContent: "(0312)-345-678" },
        { id: "31", piiCategory: "PhoneNumber", textContent: "(0812)345678" },
        { id: "32", piiCategory: "PhoneNumber", textContent: "(0812) 345678" },
        { id: "33", piiCategory: "PhoneNumber", textContent: "(0812) 345 678" },
        { id: "34", piiCategory: "PhoneNumber", textContent: "(0812)-345-678" },
        { id: "35", piiCategory: "PhoneNumber", textContent: "(0912)345678" },
        { id: "36", piiCategory: "PhoneNumber", textContent: "(0912) 345678" },
        { id: "37", piiCategory: "PhoneNumber", textContent: "(0912) 345 678" },
        { id: "38", piiCategory: "PhoneNumber", textContent: "(0912)-345-678" },
      ] as ISearchPIIHighlight[];

      const result = getFilteredByValidUkPhoneNumber(highlights);
      expect(result).toEqual(highlights);
    });
    it("Should accepts all valid 11 digit phone numbers  highlights with piiCategory `PhoneNumber`", () => {
      const highlights = [
        { id: "1", piiCategory: "PhoneNumber", textContent: "+447123456789" },
        { id: "2", piiCategory: "PhoneNumber", textContent: "+44 7123456789" },
        { id: "3", piiCategory: "PhoneNumber", textContent: "+441123456789" },
        { id: "4", piiCategory: "PhoneNumber", textContent: "+44 1123456789" },
        { id: "5", piiCategory: "PhoneNumber", textContent: "+442123456789" },
        { id: "6", piiCategory: "PhoneNumber", textContent: "+44 2123456789" },
        { id: "7", piiCategory: "PhoneNumber", textContent: "+443123456789" },
        { id: "8", piiCategory: "PhoneNumber", textContent: "+44 3123456789" },
        { id: "9", piiCategory: "PhoneNumber", textContent: "+448123456789" },
        { id: "10", piiCategory: "PhoneNumber", textContent: "+44 8123456789" },
        { id: "11", piiCategory: "PhoneNumber", textContent: "+449123456789" },
        { id: "12", piiCategory: "PhoneNumber", textContent: "+44 9123456789" },
        { id: "13", piiCategory: "PhoneNumber", textContent: "07123456789" },
        { id: "14", piiCategory: "PhoneNumber", textContent: "01123456789" },
        { id: "15", piiCategory: "PhoneNumber", textContent: "02123456789" },
        { id: "16", piiCategory: "PhoneNumber", textContent: "03123456789" },
        { id: "17", piiCategory: "PhoneNumber", textContent: "08123456789" },
        { id: "18", piiCategory: "PhoneNumber", textContent: "09123456789" },
        { id: "19", piiCategory: "PhoneNumber", textContent: "(0112)3456789" },
        {
          id: "20",
          piiCategory: "PhoneNumber",
          textContent: "(0112) 3456789",
        },
        {
          id: "21",
          piiCategory: "PhoneNumber",
          textContent: "(0112) 345 6789",
        },
        {
          id: "22",
          piiCategory: "PhoneNumber",
          textContent: "(0112)-345-6789",
        },
        { id: "23", piiCategory: "PhoneNumber", textContent: "(0212)3456789" },
        { id: "24", piiCategory: "PhoneNumber", textContent: "(0212) 3456789" },
        {
          id: "25",
          piiCategory: "PhoneNumber",
          textContent: "(0212) 345 6789",
        },
        {
          id: "26",
          piiCategory: "PhoneNumber",
          textContent: "(0212)-345-6789",
        },
        { id: "27", piiCategory: "PhoneNumber", textContent: "(0312)3456789" },
        { id: "28", piiCategory: "PhoneNumber", textContent: "(0312) 3456789" },
        {
          id: "29",
          piiCategory: "PhoneNumber",
          textContent: "(0312) 345 6789",
        },
        {
          id: "30",
          piiCategory: "PhoneNumber",
          textContent: "(0312)-345-6789",
        },
        { id: "31", piiCategory: "PhoneNumber", textContent: "(0812)3456789" },
        { id: "32", piiCategory: "PhoneNumber", textContent: "(0812) 3456789" },
        {
          id: "33",
          piiCategory: "PhoneNumber",
          textContent: "(0812) 345 6789",
        },
        {
          id: "34",
          piiCategory: "PhoneNumber",
          textContent: "(0812)-345-6789",
        },
        { id: "35", piiCategory: "PhoneNumber", textContent: "(0912)3456789" },
        { id: "36", piiCategory: "PhoneNumber", textContent: "(0912) 3456789" },
        {
          id: "37",
          piiCategory: "PhoneNumber",
          textContent: "(0912) 345 6789",
        },
        {
          id: "38",
          piiCategory: "PhoneNumber",
          textContent: "(0912)-345-6789",
        },
      ] as ISearchPIIHighlight[];

      const result = getFilteredByValidUkPhoneNumber(highlights);
      expect(result).toEqual(highlights);
    });
    it("Should filter out all invalid phone number highlights with piiCategory `PhoneNumber`", () => {
      const highlights = [
        { id: "1", piiCategory: "PhoneNumber", textContent: "+447 6789" },
        { id: "2", piiCategory: "PhoneNumber", textContent: "+789" },
        { id: "3", piiCategory: "PhoneNumber", textContent: "+44123" },
        { id: "4", piiCategory: "PhoneNumber", textContent: "+4411234567" },
        { id: "5", piiCategory: "PhoneNumber", textContent: "+4411234567890" },
        { id: "6", piiCategory: "PhoneNumber", textContent: "+4471234567" },
        { id: "7", piiCategory: "PhoneNumber", textContent: "+4471234567890" },
        { id: "8", piiCategory: "PhoneNumber", textContent: "07123 45678" },
        { id: "9", piiCategory: "PhoneNumber", textContent: "071234567890" },
        { id: "10", piiCategory: "PhoneNumber", textContent: "071234567" },
        { id: "11", piiCategory: "PhoneNumber", textContent: "456789" },
        { id: "12", piiCategory: "PhoneNumber", textContent: "01234 5678901" },
        { id: "13", piiCategory: "PhoneNumber", textContent: "01234 5678" },
        {
          id: "14",
          piiCategory: "PhoneNumber",
          textContent: "(01234) 5678901",
        },
        {
          id: "15",
          piiCategory: "PhoneNumber",
          textContent: "(01254) 895 85901",
        },
        {
          id: "17",
          piiCategory: "PhoneNumber",
          textContent: "(04254) 895 859",
        },
        {
          id: "18",
          piiCategory: "PhoneNumber",
          textContent: "(05254) 895 859",
        },
        {
          id: "19",
          piiCategory: "PhoneNumber",
          textContent: "(06254) 895 859",
        },
      ] as ISearchPIIHighlight[];

      const result = getFilteredByValidUkPhoneNumber(highlights);
      expect(result).toEqual([]);
    });
    it("Should not filter if the piiCategory is not 'PhoneNumber", () => {
      const highlights = [
        { id: "1", piiCategory: "PhoneNumber1", textContent: "+447 6789" },
        { id: "2", piiCategory: "Occupation", textContent: "+789" },
      ] as ISearchPIIHighlight[];

      const result = getFilteredByValidUkPhoneNumber(highlights);
      expect(result).toEqual(highlights);
    });
  });
});
