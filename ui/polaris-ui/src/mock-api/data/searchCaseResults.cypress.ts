import { ApiTextSearchResult } from "../../app/features/cases/domain/ApiTextSearchResult";
import { SearchCaseDataSource } from "./types/SearchCaseDataSource";

const dataSource: SearchCaseDataSource = (query: string) =>
  query === "drink" ? searchResults : [];

export default dataSource;

const searchResults: ApiTextSearchResult[] = [
  {
    id: "MTg4NDYtTUcyMCA1IEpVTkUtMS0zNA==",
    caseId: 17428,
    documentId: 1,
    pageIndex: 1,
    lineIndex: 34,
    pageHeight: 11.6806,
    pageWidth: 8.2639,
    text: "Drink drive forms roadside / hospital / station",
    words: [
      {
        boundingBox: [
          0.6007, 6.9268, 0.9447, 6.9268, 0.9447, 7.0362, 0.6007, 7.0362,
        ],
        text: "Drink",
        matchType: "Exact",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "drive",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "forms",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "roadside",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "/",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "hospital",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "/",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "station",
        matchType: "None",
        confidence: 0.0,
      },
    ],
  },
  {
    id: "MTg4NDYtTUcyMCAxMCBKVU5FLTEtMzQ=",
    caseId: 17428,
    documentId: 1,
    pageIndex: 1,
    lineIndex: 34,
    pageHeight: 11.6806,
    pageWidth: 8.2639,
    text: "Drink drink zorms zoadside / hospital / station",
    words: [
      {
        boundingBox: [
          0.6007, 6.9268, 0.9447, 6.9268, 0.9447, 7.0362, 0.6007, 7.0362,
        ],
        text: "Drink",
        matchType: "Exact",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "drink",
        matchType: "Exact",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "zorms",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "zoadside",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "/",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "hospital",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "/",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "station",
        matchType: "None",
        confidence: 0.0,
      },
    ],
  },
  {
    id: "MTg4NDYtTUcyMCAxMCBKVU5FLTEtMzQ1=",
    caseId: 17428,
    documentId: 1,
    pageIndex: 1,
    lineIndex: 34,
    pageHeight: 11.6806,
    pageWidth: 8.2639,
    text: "Drink xrive xorms xoadside / hospital / station",
    words: [
      {
        boundingBox: [
          0.6007, 6.9268, 0.9447, 6.9268, 0.9447, 7.0362, 0.6007, 7.0362,
        ],
        text: "Drink",
        matchType: "Exact",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "xrive",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "xorms",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "xoadside",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "/",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "hospital",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "/",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "station",
        matchType: "None",
        confidence: 0.0,
      },
    ],
  },
  {
    id: "MTg4NDYtU0RDIGl0ZW1zIHRvIGJlIERpc2Nsb3NlZCAoMS02KSBNQ0xPVkUtMS0xOA==",
    caseId: 17428,
    documentId: 2,
    pageIndex: 1,
    lineIndex: 18,
    pageHeight: 11.6806,
    pageWidth: 8.2639,
    text: "drink and has left in her car.",
    words: [
      {
        boundingBox: [
          1.4186, 3.9202, 1.7143, 3.9202, 1.7143, 4.0212, 1.4186, 4.0212,
        ],
        text: "drink",
        matchType: "Exact",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "and",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "has",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "left",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "in",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "her",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "car.",
        matchType: "None",
        confidence: 0.0,
      },
    ],
  },
  {
    id: "MTg4NDYtVU5VU0VEIDEgLSBTVE9STSBMT0cgMTg4MSAwMS42LjIwIC0gRURJVEVEIDIwMjAtMTEtMjMgTUNMT1ZFLTMtNDQ=",
    caseId: 17428,
    documentId: 3,
    pageIndex: 3,
    lineIndex: 44,
    pageHeight: 11.6806,
    pageWidth: 8.2639,
    text: "DRINK THE SCENE IN HER CAR IN DRINK DRINK",
    words: [
      {
        boundingBox: [
          1.0272, 4.494, 1.2966, 4.494, 1.2966, 4.5798, 1.0272, 4.5798,
        ],
        text: "DRINK",
        matchType: "Exact",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "THE",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "SCENE",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "IN",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "HER",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "CAR",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: null,
        text: "IN",
        matchType: "None",
        confidence: 0.0,
      },
      {
        boundingBox: [
          3.0021, 4.494, 3.3822, 4.494, 3.3822, 4.5798, 3.0021, 4.5798,
        ],
        text: "DRINK",
        matchType: "Exact",
        confidence: 0.0,
      },
      {
        boundingBox: [
          4.0021, 5.494, 4.3822, 5.494, 4.3822, 5.5798, 4.0021, 5.5798,
        ],
        text: "DRINK",
        matchType: "Exact",
        confidence: 0.0,
      },
    ],
  },
];
