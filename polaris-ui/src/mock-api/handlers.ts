import { rest, RestContext } from "msw";

import devSearchDataSource from "./data/searchResults.dev";
import cypressSearchDataSource from "./data/searchResults.cypress";
import devCaseDetailsDataSource from "./data/caseDetails.dev";
import cypressDetailsDataSource from "./data/caseDetails.cypress";
import devpipelinePdfResultsDataSource from "./data/pipelinePdfResults.dev";
import cypresspipelinePdfResultsDataSource from "./data/pipelinePdfResults.cypress";
import devSearchCaseDataSource from "./data/searchCaseResults.dev";
import cypressSearchCaseDataSource from "./data/searchCaseResults.cypress";
import { SearchDataSource } from "./data/types/SearchDataSource";
import {
  CaseDetailsDataSource,
  lastRequestedUrnCache,
} from "./data/types/CaseDetailsDataSource";

import { PipelinePdfResultsDataSource } from "./data/types/PipelinePdfResultsDataSource";
import { SearchCaseDataSource } from "./data/types/SearchCaseDataSource";
import * as routes from "./routes";
import { MockApiConfig } from "./MockApiConfig";

// eslint-disable-next-line import/no-webpack-loader-syntax
import pdfStrings from "./data/pdfs/pdf-strings.json";

const searchDataSources: { [key: string]: SearchDataSource } = {
  dev: devSearchDataSource,
  cypress: cypressSearchDataSource,
};

const caseDetailsDataSources: { [key: string]: CaseDetailsDataSource } = {
  dev: devCaseDetailsDataSource,
  cypress: cypressDetailsDataSource,
};

const pipelinePdfResultsDataSources: {
  [key: string]: PipelinePdfResultsDataSource;
} = {
  dev: devpipelinePdfResultsDataSource,
  cypress: cypresspipelinePdfResultsDataSource,
};

const searchCaseDataSources: { [key: string]: SearchCaseDataSource } = {
  dev: devSearchCaseDataSource,
  cypress: cypressSearchCaseDataSource,
};

export const setupHandlers = ({
  sourceName,
  maxDelayMs,
  baseUrl,
}: MockApiConfig) => {
  // make sure we are reading a number not string from config
  //  also msw will not accept a delay of 0, so if 0 is passed then just set to 1ms
  const sanitisedMaxDelay = Number(maxDelayMs) || 1;
  const callStack = { TRACKER_ROUTE: 0, INITIATE_PIPELINE_ROUTE: 0 };

  const makeApiPath = (path: string) => new URL(path, baseUrl).toString();

  const delay = (ctx: RestContext) =>
    ctx.delay(Math.random() * sanitisedMaxDelay);

  return [
    rest.get(makeApiPath(routes.CASE_SEARCH_ROUTE), (req, res, ctx) => {
      const { urn } = req.params;
      lastRequestedUrnCache.urn = urn;

      const results = searchDataSources[sourceName](urn);

      return res(delay(ctx), ctx.json(results));
    }),

    rest.get(makeApiPath(routes.CASE_ROUTE), (req, res, ctx) => {
      const { caseId } = req.params;

      const result = {
        ...caseDetailsDataSources[sourceName](+caseId),
        uniqueReferenceNumber: lastRequestedUrnCache.urn || "99ZZ9999999",
      };

      return res(delay(ctx), ctx.json(result));
    }),

    rest.post(makeApiPath(routes.INITIATE_PIPELINE_ROUTE), (req, res, ctx) => {
      const { caseId, urn } = req.params;
      return res(
        delay(ctx),
        ctx.json({
          trackerUrl: makeApiPath(`api/urns/${urn}/cases/${caseId}/tracker`),
        })
      );
    }),

    rest.put(makeApiPath(routes.SAVE_REDACTION_ROUTE), (req, res, ctx) => {
      return res(ctx.json({}));
      // return res(ctx.status(500));
    }),

    rest.get(makeApiPath(routes.TRACKER_ROUTE), (req, res, ctx) => {
      // always maxDelay as we want this to be slow to illustrate async nature of tracker/polling
      //  (when in dev mode)
      callStack["TRACKER_ROUTE"]++;
      const result = pipelinePdfResultsDataSources[sourceName]();
      if (callStack["TRACKER_ROUTE"] > result.length) {
        return res(ctx.delay(sanitisedMaxDelay), ctx.json(result[0]));
      }
      return res(
        ctx.delay(sanitisedMaxDelay),
        ctx.json(result[callStack["TRACKER_ROUTE"] - 1])
      );
    }),

    rest.get(makeApiPath(routes.TEXT_SEARCH_ROUTE), (req, res, ctx) => {
      const query = req.url.searchParams.get("query")!;
      const results = searchCaseDataSources[sourceName](query);

      return res(delay(ctx), ctx.json(results));
    }),

    rest.get(makeApiPath(routes.FILE_ROUTE), (req, res, ctx) => {
      const { documentId } = req.params;

      const blobName = pipelinePdfResultsDataSources[
        sourceName
      ]()[0].documents.find(
        (document) => document.documentId === documentId
      )?.pdfBlobName;

      const fileBase64 = (pdfStrings as { [key: string]: string })[blobName!];

      return res(delay(ctx), ctx.body(_base64ToArrayBuffer(fileBase64)));
    }),

    rest.get(makeApiPath(routes.GET_SAS_URL_ROUTE), (req, res, ctx) => {
      const { documentId } = req.params;

      const blobName = pipelinePdfResultsDataSources[
        sourceName
      ]()[0].documents.find(
        (document) => document.documentId === documentId
      )?.pdfBlobName;

      return res(
        ctx.text(
          makeApiPath(routes.SAS_URL_ROUTE).replace(":blobName", blobName!)
        )
      );
    }),

    rest.post(makeApiPath(routes.DOCUMENT_CHECKOUT_ROUTE), (req, res, ctx) => {
      return res(ctx.json({ successful: true, documentStatus: "CheckedOut" }));
      // return res(ctx.status(500));
    }),

    rest.delete(makeApiPath(routes.DOCUMENT_CHECKIN_ROUTE), (req, res, ctx) => {
      return res(ctx.json({ successful: true, documentStatus: "CheckedIn" }));
    }),
  ];
};

function _base64ToArrayBuffer(base64: string) {
  var binary_string = window.atob(base64);
  var len = binary_string.length;
  var bytes = new Uint8Array(len);
  for (var i = 0; i < len; i++) {
    bytes[i] = binary_string.charCodeAt(i);
  }
  return bytes.buffer;
}
