import { CombinedState } from "../../domain/CombinedState";
import { reducer } from "./reducer";
import * as accordionMapper from "./map-accordion-state";
import * as documentsMapper from "./map-documents-state";
import * as notificationMapper from "./map-notification-state";
import * as apiGateway from "../../api/gateway-api";
import { ApiResult } from "../../../../common/types/ApiResult";
import { PipelineResults } from "../../domain/gateway/PipelineResults";
import * as sorter from "./sort-mapped-text-search-result";
import { MappedTextSearchResult } from "../../domain/MappedTextSearchResult";
import { MappedDocumentResult } from "../../domain/MappedDocumentResult";
import * as documentVisibility from "./is-document-visible";
import { ApiTextSearchResult } from "../../domain/gateway/ApiTextSearchResult";
import * as textSearchMapper from "./map-text-search";
import * as documentNameSearchMapper from "./map-document-name-search";
import * as combineDocumentNameMatches from "./combine-document-name-matches";
import * as filters from "./map-filters";
import * as missingDocuments from "./map-missing-documents";
import { MappedCaseDocument } from "../../domain/MappedCaseDocument";
import { IPdfHighlight } from "../../domain/IPdfHighlight";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import * as sanitizeSearchTerm from "./sanitizeSearchTerm";
import * as shouldTriggerPipelineRefresh from "../utils/shouldTriggerPipelineRefresh";
import * as filterApiResults from "./filter-api-results";
import {
  buildDefaultNotificationState,
  NotificationState,
} from "../../domain/NotificationState";
import * as notificationsMappingFunctions from "./map-notification-state";
import * as mapNotificationToDocumentsState from "./map-notification-to-documents-state";
import { AsyncResult } from "../../../../common/types/AsyncResult";
import { AccordionData } from "../../presentation/case-details/accordion/types";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
} from "../../domain/redactionLog/RedactionLogData";
import { IPageDeleteRedaction } from "../../domain//IPageDeleteRedaction";

const ERROR = new Error();

jest.mock("../../../../config", () => ({
  FEATURE_FLAG_BACKGROUND_PIPELINE_REFRESH: true,
}));

describe("useCaseDetailsState reducer", () => {
  afterEach(() => {
    jest.restoreAllMocks();
    jest.useRealTimers();
  });

  describe("UPDATE_CASE_DETAILS", () => {
    it("throws if update case details fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_CASE_DETAILS",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
          },
        })
      ).toThrowError(ERROR);
    });

    it("can update case details", () => {
      const newCaseState = {} as CombinedState["caseState"];

      const nextState = reducer({} as CombinedState, {
        type: "UPDATE_CASE_DETAILS",
        payload: newCaseState,
      });

      expect(nextState.caseState).toBe(newCaseState);
    });
  });

  describe("UPDATE_REDACTION_LOG_LOOK_UPS_DATA", () => {
    it("should not update the state if the redaction log looks up data call fails", () => {
      const nextState = reducer({} as CombinedState, {
        type: "UPDATE_REDACTION_LOG_LOOK_UPS_DATA",
        payload: {
          status: "failed",
          error: ERROR,
          httpStatusCode: undefined,
        },
      });
      expect(nextState).toStrictEqual({});
    });
    it("can update redactionLog redactionLogLookUpsData", () => {
      const newCaseState = {} as AsyncResult<RedactionLogLookUpsData>;

      const nextState = reducer({} as CombinedState, {
        type: "UPDATE_REDACTION_LOG_LOOK_UPS_DATA",
        payload: newCaseState,
      });

      expect(nextState).toStrictEqual({
        redactionLog: { redactionLogLookUpsData: {} },
      });
    });
  });

  describe("UPDATE_REDACTION_LOG_MAPPING_DATA", () => {
    it("should not update the state if the redaction log mapping data call fails", () => {
      const nextState = reducer({} as CombinedState, {
        type: "UPDATE_REDACTION_LOG_MAPPING_DATA",
        payload: {
          status: "failed",
          error: ERROR,
          httpStatusCode: undefined,
        },
      });
      expect(nextState).toStrictEqual({});
    });
    it("can update redactionLog redactionLogMappingData", () => {
      const newCaseState = {} as AsyncResult<RedactionLogMappingData>;

      const nextState = reducer({} as CombinedState, {
        type: "UPDATE_REDACTION_LOG_MAPPING_DATA",
        payload: newCaseState,
      });

      expect(nextState).toStrictEqual({
        redactionLog: { redactionLogMappingData: {} },
      });
    });
  });

  describe("UPDATE_DOCUMENTS", () => {
    it("throws error if update documents fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_DOCUMENTS",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
          },
        })
      ).toThrowError(ERROR);
    });
    it("returns the currentState if the payload.status is loading", () => {
      const existingState = {} as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_DOCUMENTS",
        payload: {
          status: "loading",
        },
      });
      expect(nextState).toStrictEqual(existingState);
    });

    it("should update the states correctly if there are no open document tabs", () => {
      const expectedDocumentsState: AsyncResult<MappedCaseDocument[]> = {
        status: "succeeded",
        data: [],
      };
      const mockDocumentsState = {} as CombinedState["documentsState"];
      const mockNotificationState = {
        name: "mock_notification",
      } as unknown as CombinedState["notificationState"];
      const mockAccordionState = {
        name: "mock_accordion",
      } as unknown as CombinedState["accordionState"];

      jest
        .spyOn(
          mapNotificationToDocumentsState,
          "mapNotificationToDocumentsState"
        )
        .mockImplementation(() => {
          return expectedDocumentsState;
        });

      jest
        .spyOn(documentsMapper, "mapDocumentsState")
        .mockImplementation(() => {
          return mockDocumentsState;
        });
      jest
        .spyOn(notificationMapper, "mapNotificationState")
        .mockImplementation(() => {
          return mockNotificationState;
        });
      jest
        .spyOn(accordionMapper, "mapAccordionState")
        .mockImplementation(() => {
          return mockAccordionState;
        });
      const existingState = {
        notificationState: {},
        documentsState: {},
        caseState: { name: "caseState" },
        documentRefreshData: { savedDocumentDetails: [] },
      } as unknown as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_DOCUMENTS",
        payload: {
          status: "succeeded",
          data: [],
        },
      });

      expect(documentsMapper.mapDocumentsState).toHaveBeenCalledTimes(1);
      expect(documentsMapper.mapDocumentsState).toHaveBeenCalledWith([], []);
      expect(notificationMapper.mapNotificationState).toHaveBeenCalledTimes(1);
      expect(
        mapNotificationToDocumentsState.mapNotificationToDocumentsState
      ).toHaveBeenCalledTimes(1);
      expect(
        mapNotificationToDocumentsState.mapNotificationToDocumentsState
      ).toHaveBeenCalledWith(
        {
          name: "mock_notification",
        },
        {}
      );
      expect(accordionMapper.mapAccordionState).toHaveBeenCalledTimes(1);
      expect(accordionMapper.mapAccordionState).toHaveBeenCalledWith(
        {
          data: [],
          status: "succeeded",
        },
        undefined
      );

      expect(nextState).toStrictEqual({
        accordionState: { name: "mock_accordion" },
        caseState: { name: "caseState" },
        documentRefreshData: { savedDocumentDetails: [] },
        documentsState: {
          data: [],
          status: "succeeded",
        },
        notificationState: { name: "mock_notification" },
      });
    });
  });

  describe("UPDATE_PIPELINE", () => {
    it("throws if update pipelineState fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
            correlationId: "corId_1",
          },
        })
      ).toThrowError(ERROR);
    });

    it("should update the state if pipeline is initiating", () => {
      const existingPipelineState = {
        status: "complete",
        data: {} as PipelineResults,
        correlationId: "abc",
      } as CombinedState["pipelineState"];

      const nextState = reducer(
        {
          pipelineState: existingPipelineState,
        } as CombinedState,
        {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "initiating",
            correlationId: "abc",
          },
        }
      );

      expect(nextState.pipelineState).toEqual({
        status: "initiating",
        data: {} as PipelineResults,
        correlationId: "abc",
      });
    });

    it("Should update the state if the pipeline is incomplete", () => {
      const dateSpy = jest
        .spyOn(Date.prototype, "toISOString")
        .mockReturnValue("2023-01-01T00:00:00.000Z");
      const existingPipelineState = {
        status: "initiating",
        data: {
          processingCompleted: "time_pc1",
          documentsRetrieved: "time_dc1",
          status: "DocumentsRetrieved",
          documents: [
            {
              documentId: "1",
              conversionStatus: "PdfEncrypted",
              status: "New",
            },
            {
              documentId: "2",
              conversionStatus: "DocumentConverted",
              status: "Indexed",
            },
          ],
        } as PipelineResults,
        correlationId: "abc",
      } as CombinedState["pipelineState"];

      const existingLocalDocumentState = {
        "1": { conversionStatus: "PdfEncrypted" },
        "2": { conversionStatus: "DocumentConverted" },
      } as CombinedState["localDocumentState"];

      const existingPipelineRefreshData = {
        startPipelineRefresh: false,
        lastProcessingCompleted: "time_lpc1",
        localLastRefreshTime: "abc",
      } as CombinedState["pipelineRefreshData"];

      const nextStateIncompleteStatus = reducer(
        {
          pipelineState: existingPipelineState,
          localDocumentState: existingLocalDocumentState,
          pipelineRefreshData: existingPipelineRefreshData,
        } as CombinedState,
        {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "incomplete",
            data: {
              documents: [
                {
                  documentId: "1",
                  conversionStatus: "DocumentConverted",
                  status: "New",
                },
                {
                  documentId: "2",
                  conversionStatus: "DocumentConverted",
                  status: "Indexed",
                },
                {
                  documentId: "3",
                  conversionStatus: "DocumentConverted",
                  status: "Indexed",
                },
              ],
              documentsRetrieved: "time_dc2",
              processingCompleted: "time_pc2",
              status: "DocumentsRetrieved",
            },
            correlationId: "abc",
          },
        }
      );

      expect(nextStateIncompleteStatus).toEqual({
        pipelineState: {
          status: "incomplete",
          data: {
            documents: [
              {
                documentId: "1",
                conversionStatus: "DocumentConverted",
                status: "New",
              },
              {
                documentId: "2",
                conversionStatus: "DocumentConverted",
                status: "Indexed",
              },
              {
                documentId: "3",
                conversionStatus: "DocumentConverted",
                status: "Indexed",
              },
            ],
            documentsRetrieved: "time_dc2",
            processingCompleted: "time_pc2",
            status: "DocumentsRetrieved",
          },
          correlationId: "abc",
        },
        pipelineRefreshData: {
          startPipelineRefresh: false,
          lastProcessingCompleted: "time_pc2",
          localLastRefreshTime: "abc",
        },
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
          "2": { conversionStatus: "DocumentConverted" },
          "3": { conversionStatus: "DocumentConverted" },
        },
      });

      dateSpy.mockRestore();
    });

    it("Should update the state if the pipeline is complete and pipelineresult status is Completed", () => {
      const dateSpy = jest
        .spyOn(Date.prototype, "toISOString")
        .mockReturnValue("2023-01-01T00:00:00.000Z");
      const existingPipelineState = {
        status: "initiating",
        data: {
          processingCompleted: "time_pc1",
          documentsRetrieved: "time_dc1",
          status: "DocumentsRetrieved",
          documents: [
            {
              documentId: "1",
              conversionStatus: "PdfEncrypted",
              status: "New",
            },
            {
              documentId: "2",
              conversionStatus: "DocumentConverted",
              status: "Indexed",
            },
          ],
        } as PipelineResults,
        correlationId: "abc",
      } as CombinedState["pipelineState"];

      const existingLocalDocumentState = {
        "1": { conversionStatus: "PdfEncrypted" },
        "2": { conversionStatus: "DocumentConverted" },
      } as CombinedState["localDocumentState"];

      const existingPipelineRefreshData = {
        startPipelineRefresh: false,
        lastProcessingCompleted: "time_lpc1",
        localLastRefreshTime: "abc",
      } as CombinedState["pipelineRefreshData"];

      const nextStateCompleteStatus = reducer(
        {
          pipelineState: existingPipelineState,
          localDocumentState: existingLocalDocumentState,
          pipelineRefreshData: existingPipelineRefreshData,
        } as CombinedState,
        {
          type: "UPDATE_PIPELINE",
          payload: {
            status: "complete",
            data: {
              documents: [
                {
                  documentId: "1",
                  conversionStatus: "DocumentConverted",
                  status: "New",
                },
                {
                  documentId: "2",
                  conversionStatus: "DocumentConverted",
                  status: "Indexed",
                },
                {
                  documentId: "3",
                  conversionStatus: "DocumentConverted",
                  status: "Indexed",
                },
              ],
              documentsRetrieved: "time_dc2",
              processingCompleted: "time_pc2",
              status: "Completed",
            },
            correlationId: "abc",
          },
        }
      );

      expect(nextStateCompleteStatus).toEqual({
        pipelineState: {
          status: "complete",
          data: {
            documents: [
              {
                documentId: "1",
                conversionStatus: "DocumentConverted",
                status: "New",
              },
              {
                documentId: "2",
                conversionStatus: "DocumentConverted",
                status: "Indexed",
              },
              {
                documentId: "3",
                conversionStatus: "DocumentConverted",
                status: "Indexed",
              },
            ],
            documentsRetrieved: "time_dc2",
            processingCompleted: "time_pc2",
            status: "Completed",
          },
          correlationId: "abc",
        },
        pipelineRefreshData: {
          startPipelineRefresh: false,
          lastProcessingCompleted: "time_pc2",
          localLastRefreshTime: "2023-01-01T00:00:00.000Z",
        },
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
          "2": { conversionStatus: "DocumentConverted" },
          "3": { conversionStatus: "DocumentConverted" },
        },
      });

      dateSpy.mockRestore();
    });
  });

  describe("UPDATE_DOCUMENT_REFRESH", () => {
    it("can update documentRefreshData", () => {
      const dateSpy = jest
        .spyOn(Date.prototype, "toISOString")
        .mockReturnValue("2023-01-01T00:00:00.000Z");
      const existingState = {
        documentRefreshData: {
          startDocumentRefresh: false,
          savedDocumentDetails: [{ documentId: "1", versionId: 1 }],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_DOCUMENT_REFRESH",
        payload: {
          startDocumentRefresh: true,
          savedDocumentDetails: {
            documentId: "2",
            versionId: 1,
          },
        },
      });

      expect(result).toEqual({
        documentRefreshData: {
          startDocumentRefresh: true,
          savedDocumentDetails: [
            { documentId: "1", versionId: 1 },
            { documentId: "2", versionId: 1 },
          ],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      });

      dateSpy.mockRestore();
    });
  });

  describe("UPDATE_DOCUMENTS", () => {
    it("throws error if update documents fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_DOCUMENTS",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
          },
        })
      ).toThrowError(ERROR);
    });
    it("returns the currentState if the payload.status is loading", () => {
      const existingState = {} as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_DOCUMENTS",
        payload: {
          status: "loading",
        },
      });
      expect(nextState).toStrictEqual(existingState);
    });
    it("can update documentRefreshData if the payload doesn't have savedDocumentDetails ", () => {
      const existingState = {
        documentRefreshData: {
          startDocumentRefresh: false,
          savedDocumentDetails: [{ documentId: "1", versionId: 1 }],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_DOCUMENT_REFRESH",
        payload: {
          startDocumentRefresh: true,
        },
      });

      expect(result).toEqual({
        documentRefreshData: {
          startDocumentRefresh: true,
          savedDocumentDetails: [{ documentId: "1", versionId: 1 }],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      });
    });
  });

  describe("UPDATE_PIPELINE_REFRESH", () => {
    it("can update pipelineRefreshData", () => {
      const existingState = {
        pipelineRefreshData: {
          startPipelineRefresh: false,
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
          localLastRefreshTime: "2023-04-05T15:02:17.601Z",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_PIPELINE_REFRESH",
        payload: {
          startPipelineRefresh: true,
        },
      });

      expect(result).toEqual({
        pipelineRefreshData: {
          startPipelineRefresh: true,
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
          localLastRefreshTime: "2023-04-05T15:02:17.601Z",
        },
      });
    });

    it("should update the states correctly if there are no open document tabs", () => {
      const expectedDocumentsState: AsyncResult<MappedCaseDocument[]> = {
        status: "succeeded",
        data: [],
      };
      const mockDocumentsState = {} as CombinedState["documentsState"];
      const mockNotificationState = {
        name: "mock_notification",
      } as unknown as CombinedState["notificationState"];
      const mockAccordionState = {
        name: "mock_accordion",
      } as unknown as CombinedState["accordionState"];

      jest
        .spyOn(
          mapNotificationToDocumentsState,
          "mapNotificationToDocumentsState"
        )
        .mockImplementation(() => {
          return expectedDocumentsState;
        });

      jest
        .spyOn(documentsMapper, "mapDocumentsState")
        .mockImplementation(() => {
          return mockDocumentsState;
        });
      jest
        .spyOn(notificationMapper, "mapNotificationState")
        .mockImplementation(() => {
          return mockNotificationState;
        });
      jest
        .spyOn(accordionMapper, "mapAccordionState")
        .mockImplementation(() => {
          return mockAccordionState;
        });
      const existingState = {
        notificationState: {},
        documentsState: {},
        caseState: { name: "caseState" },
        documentRefreshData: { savedDocumentDetails: [] },
      } as unknown as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_DOCUMENTS",
        payload: {
          status: "succeeded",
          data: [],
        },
      });

      expect(documentsMapper.mapDocumentsState).toHaveBeenCalledTimes(1);
      expect(documentsMapper.mapDocumentsState).toHaveBeenCalledWith([], []);
      expect(notificationMapper.mapNotificationState).toHaveBeenCalledTimes(1);
      expect(
        mapNotificationToDocumentsState.mapNotificationToDocumentsState
      ).toHaveBeenCalledTimes(1);
      expect(
        mapNotificationToDocumentsState.mapNotificationToDocumentsState
      ).toHaveBeenCalledWith(
        {
          name: "mock_notification",
        },
        {}
      );
      expect(accordionMapper.mapAccordionState).toHaveBeenCalledTimes(1);
      expect(accordionMapper.mapAccordionState).toHaveBeenCalledWith(
        {
          data: [],
          status: "succeeded",
        },
        undefined
      );

      expect(nextState).toStrictEqual({
        accordionState: { name: "mock_accordion" },
        caseState: { name: "caseState" },
        documentRefreshData: { savedDocumentDetails: [] },
        documentsState: {
          data: [],
          status: "succeeded",
        },
        notificationState: { name: "mock_notification" },
      });
    });
  });

  describe("OPEN_PDF", () => {
    it("can try to open a tab when the documents are unknown", () => {
      const nextState = reducer(
        {
          documentsState: { status: "loading" },
          tabsState: {
            headers: {
              Authorization: "foo",
              "Correlation-Id": "foo1",
            } as HeadersInit,
          },
        } as CombinedState,
        {
          type: "OPEN_PDF",
          payload: {
            documentId: "1",
            mode: "read",
            headers: { Authorization: "bar", "Correlation-Id": "bar1" },
          },
        }
      );

      expect(nextState).toEqual({
        documentsState: { status: "loading" },
        searchState: {
          isResultsVisible: false,
        },
        tabsState: {
          headers: {
            Authorization: "bar",
            "Correlation-Id": "bar1",
          } as HeadersInit,
        },
      } as CombinedState);
    });

    it("can open a tab when the pdf details are known", () => {
      const existingTabsState = {
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [],
        activeTabId: "",
      } as CombinedState["tabsState"];

      const existingDocumentsState = {
        status: "succeeded",
        data: [{ documentId: "1" }],
      } as CombinedState["documentsState"];

      const existingPipelineState = {
        status: "complete",
        data: {
          documents: [{ documentId: "1" }],
        },
      } as CombinedState["pipelineState"];

      jest
        .spyOn(apiGateway, "resolvePdfUrl")
        .mockImplementation((urn, caseId, documentId) => {
          if (urn !== "bar" || caseId !== 99 || documentId !== "1")
            throw new Error();
          return "baz";
        });

      const nextState = reducer(
        {
          documentsState: existingDocumentsState,
          pipelineState: existingPipelineState,
          tabsState: existingTabsState,
          notificationState: buildDefaultNotificationState(),
          urn: "bar",
          caseId: 99,
        } as CombinedState,
        {
          type: "OPEN_PDF",
          payload: {
            documentId: "1",
            mode: "read",
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        }
      );

      expect(nextState.tabsState).toEqual({
        headers: {
          Authorization: "bar",
          "Correlation-Id": "bar1",
        },
        items: [
          {
            documentId: "1",
            clientLockedState: "unlocked",
            areaOnlyRedactionMode: false,
            mode: "read",

            redactionHighlights: [],
            pageDeleteRedactions: [],
            pageRotations: [],
            rotatePageMode: false,
            deletePageMode: true,
            url: "baz",
            isDeleted: false,
            saveStatus: {
              status: "initial",
              type: "none",
            },
          },
        ],
        activeTabId: "",
      });
    });

    it("can open a tab when the pdf details are not known", () => {
      const existingTabsState = {
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [],
        activeTabId: "",
      } as CombinedState["tabsState"];

      const existingDocumentsState = {
        status: "succeeded",
        data: [{ documentId: "1", versionId: 2 }],
      } as CombinedState["documentsState"];

      const existingPipelineState = {
        status: "incomplete",
      } as CombinedState["pipelineState"];

      const nextState = reducer(
        {
          urn: "abc-urn",
          caseId: 123,
          documentsState: existingDocumentsState,
          pipelineState: existingPipelineState,
          tabsState: existingTabsState,
          notificationState: buildDefaultNotificationState(),
        } as CombinedState,
        {
          type: "OPEN_PDF",
          payload: {
            documentId: "1",

            mode: "read",
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        }
      );

      expect(nextState.tabsState).toEqual({
        headers: {
          Authorization: "bar",
          "Correlation-Id": "bar1",
        } as HeadersInit,
        items: [
          {
            documentId: "1",
            versionId: 2,
            clientLockedState: "unlocked",
            areaOnlyRedactionMode: false,
            url: "http://localhost/api/urns/abc-urn/cases/123/documents/1/versions/2/pdf",
            isDeleted: false,
            saveStatus: {
              status: "initial",
              type: "none",
            },
            redactionHighlights: [],
            pageDeleteRedactions: [],
            pageRotations: [],
            rotatePageMode: false,
            deletePageMode: true,
            mode: "read",
          },
        ],
        activeTabId: "",
      });
    });

    describe("reopening pdfs", () => {
      it("can reopen a read mode pdf and show the previously visible document", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [{ documentId: "1", mode: "read" }],
        } as CombinedState["tabsState"];

        const nextState = reducer(
          {
            documentsState: { status: "succeeded" },
            tabsState: existingTabsState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              documentId: "1",
              mode: "read",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          documentsState: { status: "succeeded" },
          searchState: {
            isResultsVisible: false,
          },
          tabsState: {
            ...existingTabsState,
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        });
      });

      it("can reopen a search mode pdf and show the previously visible document", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [{ documentId: "1", mode: "search", searchTerm: "foo" }],
        } as CombinedState["tabsState"];

        const nextState = reducer(
          {
            searchState: { submittedSearchTerm: "foo" },
            documentsState: { status: "succeeded" },
            tabsState: existingTabsState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              documentId: "1",
              mode: "search",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          documentsState: { status: "succeeded" },
          searchState: {
            submittedSearchTerm: "foo",
            isResultsVisible: false,
          },
          tabsState: {
            ...existingTabsState,
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
          },
        });
      });

      it("can reopen a read mode pdf in search mode", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [
            { documentId: "0", mode: "read" },
            { documentId: "1", mode: "read" },
            { documentId: "2", mode: "read" },
          ],
        } as CombinedState["tabsState"];

        const existingDocumentsState = {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        } as CombinedState["documentsState"];

        const existingSearchState = {
          submittedSearchTerm: "foo",
          searchConfigs: {
            documentContent: {
              results: {
                status: "succeeded",
                data: {
                  documentResults: [
                    {
                      documentId: "1",
                      occurrences: [
                        {
                          pageIndex: 0,
                          pageHeight: 11.69,
                          pageWidth: 8.27,

                          occurrencesInLine: [[21, 21, 9, 9, 23, 23, 9, 9]],
                        },
                      ] as MappedDocumentResult["occurrences"],
                      occurrencesInDocumentCount: 3,
                    },
                  ],
                },
              },
            },
          },
        } as CombinedState["searchState"];

        const existingPipelineState = {} as CombinedState["pipelineState"];

        const existingNotificationState = buildDefaultNotificationState();

        const nextState = reducer(
          {
            searchState: existingSearchState,
            documentsState: existingDocumentsState,
            tabsState: existingTabsState,
            pipelineState: existingPipelineState,
            notificationState: existingNotificationState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              documentId: "1",
              mode: "search",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          searchState: {
            submittedSearchTerm: "foo",
            searchConfigs: {
              documentContent: {
                results: {
                  status: "succeeded",
                  data: {
                    documentResults: [
                      {
                        documentId: "1",
                        occurrences: [
                          {
                            pageIndex: 0,
                            pageHeight: 11.69,
                            pageWidth: 8.27,

                            occurrencesInLine: [[21, 21, 9, 9, 23, 23, 9, 9]],
                          },
                        ],
                        occurrencesInDocumentCount: 3,
                      },
                    ],
                  },
                },
              },
            },
            isResultsVisible: false,
          },
          documentsState: { status: "succeeded", data: [] },
          tabsState: {
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
            items: [
              {
                documentId: "0",
                mode: "read",
              },
              {
                documentId: "1",
                mode: "search",
                clientLockedState: "unlocked",
                searchTerm: "foo",
                occurrencesInDocumentCount: 3,
                pageDeleteRedactions: [],
                pageRotations: [],
                rotatePageMode: false,
                deletePageMode: true,
                areaOnlyRedactionMode: false,
                searchHighlights: [
                  {
                    id: "0",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 0,
                      boundingRect: {
                        x1: 20.97,
                        x2: 23.03,
                        y1: 20.97,
                        y2: 23.03,
                        width: 8.27,
                        height: 11.69,
                      },
                      rects: [
                        {
                          x1: 20.97,
                          x2: 23.03,
                          y1: 20.97,
                          y2: 23.03,
                          width: 8.27,
                          height: 11.69,
                        },
                      ],
                    },
                  },
                ],
                isDeleted: false,
                saveStatus: {
                  status: "initial",
                  type: "none",
                },
              },
              {
                documentId: "2",
                mode: "read",
              },
            ],
          },
          pipelineState: {},
          notificationState: existingNotificationState,
        });
      });

      it("can reopen a search mode pdf in read mode", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [
            { documentId: "d0", mode: "read" },
            { documentId: "1", mode: "search" },
            { documentId: "2", mode: "read" },
          ],
        } as CombinedState["tabsState"];

        const existingDocumentsState = {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        } as CombinedState["documentsState"];

        const existingSearchState = {
          submittedSearchTerm: "foo",
          searchConfigs: {
            documentContent: {
              results: {
                status: "succeeded",
              },
            },
          },
        } as CombinedState["searchState"];

        const existingPipelineState = {} as CombinedState["pipelineState"];

        const existingNotificationState = buildDefaultNotificationState();

        const nextState = reducer(
          {
            searchState: existingSearchState,
            documentsState: existingDocumentsState,
            tabsState: existingTabsState,
            pipelineState: existingPipelineState,
            notificationState: existingNotificationState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              documentId: "1",
              mode: "read",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          documentsState: existingDocumentsState,
          searchState: { ...existingSearchState, isResultsVisible: false },
          pipelineState: existingPipelineState,
          notificationState: existingNotificationState,
          tabsState: {
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
            items: [
              { documentId: "d0", mode: "read" },
              {
                documentId: "1",
                areaOnlyRedactionMode: false,
                clientLockedState: "unlocked",
                mode: "read",
                url: undefined,
                isDeleted: false,
                saveStatus: {
                  status: "initial",
                  type: "none",
                },
                pageDeleteRedactions: [],
                pageRotations: [],
                rotatePageMode: false,
                deletePageMode: true,
              },
              { documentId: "2", mode: "read" },
            ],
          },
        });
      });

      it("can reopen a search mode pdf in search mode with a different search term", () => {
        const existingTabsState = {
          headers: {
            Authorization: "foo",
            "Correlation-Id": "foo1",
          } as HeadersInit,
          items: [
            { documentId: "d0", mode: "read" },
            {
              documentId: "1",
              mode: "search",
              searchTerm: "foo",
              occurrencesInDocumentCount: 1,
              pageOccurrences: [
                {
                  boundingBoxes: [[1, 2, 3]],
                  pageIndex: 0,
                  pageHeight: 11.69,
                  pageWidth: 8.27,
                },
              ],
            },
            { documentId: "2", mode: "read" },
          ],
        } as CombinedState["tabsState"];

        const existingDocumentsState = {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        } as CombinedState["documentsState"];

        const existingSearchState = {
          submittedSearchTerm: "bar",
          searchConfigs: {
            documentContent: {
              results: {
                status: "succeeded",
                data: {
                  documentResults: [
                    {
                      documentId: "1",
                      occurrences: [
                        {
                          pageIndex: 1,
                          pageHeight: 8,
                          pageWidth: 7,
                          occurrencesInLine: [[1, 1, 9, 9, 2, 2]],
                        },
                        {
                          pageIndex: 2,
                          pageHeight: 9,
                          pageWidth: 8,
                          occurrencesInLine: [[2, 2, 9, 9, 3, 3]],
                        },
                        {
                          pageIndex: 2,
                          pageHeight: 10,
                          pageWidth: 9,
                          occurrencesInLine: [[3, 3, 9, 9, 4, 4]],
                        },
                      ] as MappedDocumentResult["occurrences"],
                      occurrencesInDocumentCount: 4,
                    },
                  ],
                },
              },
            },
          },
        } as CombinedState["searchState"];

        const existingPipelineState = {} as CombinedState["pipelineState"];
        const existingNotificationState = buildDefaultNotificationState();

        const nextState = reducer(
          {
            searchState: existingSearchState,
            documentsState: existingDocumentsState,
            tabsState: existingTabsState,
            pipelineState: existingPipelineState,
            notificationState: existingNotificationState,
          } as CombinedState,
          {
            type: "OPEN_PDF",
            payload: {
              documentId: "1",
              mode: "search",
              headers: {
                Authorization: "bar",
                "Correlation-Id": "bar1",
              } as HeadersInit,
            },
          }
        );

        expect(nextState).toEqual({
          searchState: {
            submittedSearchTerm: "bar",
            searchConfigs: {
              documentContent: {
                results: {
                  status: "succeeded",
                  data: {
                    documentResults: [
                      {
                        documentId: "1",
                        occurrences: [
                          {
                            pageIndex: 1,
                            pageHeight: 8,
                            pageWidth: 7,
                            occurrencesInLine: [[1, 1, 9, 9, 2, 2]],
                          },
                          {
                            pageIndex: 2,
                            pageHeight: 9,
                            pageWidth: 8,
                            occurrencesInLine: [[2, 2, 9, 9, 3, 3]],
                          },
                          {
                            pageIndex: 2,
                            pageHeight: 10,
                            pageWidth: 9,
                            occurrencesInLine: [[3, 3, 9, 9, 4, 4]],
                          },
                        ],
                        occurrencesInDocumentCount: 4,
                      },
                    ],
                  },
                },
              },
            },
            isResultsVisible: false,
          },
          documentsState: { status: "succeeded", data: [] },
          tabsState: {
            headers: {
              Authorization: "bar",
              "Correlation-Id": "bar1",
            } as HeadersInit,
            items: [
              { documentId: "d0", mode: "read" },
              {
                documentId: "1",
                mode: "search",
                searchTerm: "bar",
                isDeleted: false,
                saveStatus: {
                  status: "initial",
                  type: "none",
                },
                areaOnlyRedactionMode: false,
                occurrencesInDocumentCount: 4,
                pageDeleteRedactions: [],
                pageRotations: [],
                rotatePageMode: false,
                deletePageMode: true,
                pageOccurrences: [
                  {
                    boundingBoxes: [[1, 2, 3]],
                    pageIndex: 0,
                    pageHeight: 11.69,
                    pageWidth: 8.27,
                  },
                ],
                clientLockedState: "unlocked",
                searchHighlights: [
                  {
                    id: "0",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 1,
                      boundingRect: {
                        x1: 0.97,
                        y1: 0.97,
                        x2: 2.03,
                        y2: 2.03,
                        width: 7,
                        height: 8,
                      },
                      rects: [
                        {
                          x1: 0.97,
                          y1: 0.97,
                          x2: 2.03,
                          y2: 2.03,
                          width: 7,
                          height: 8,
                        },
                      ],
                    },
                  },
                  {
                    id: "1",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 2,
                      boundingRect: {
                        x1: 1.97,
                        y1: 1.97,
                        x2: 3.03,
                        y2: 3.03,
                        width: 8,
                        height: 9,
                      },
                      rects: [
                        {
                          x1: 1.97,
                          y1: 1.97,
                          x2: 3.03,
                          y2: 3.03,
                          width: 8,
                          height: 9,
                        },
                      ],
                    },
                  },
                  {
                    id: "2",
                    type: "search",
                    highlightType: "linear",
                    position: {
                      pageNumber: 2,
                      boundingRect: {
                        x1: 2.97,
                        y1: 2.97,
                        x2: 4.03,
                        y2: 4.03,
                        width: 8,
                        height: 9,
                      },
                      rects: [
                        {
                          x1: 2.97,
                          y1: 2.97,
                          x2: 4.03,
                          y2: 4.03,
                          width: 8,
                          height: 9,
                        },
                      ],
                    },
                  },
                ],
              },
              { documentId: "2", mode: "read" },
            ],
          },
          pipelineState: {},
          notificationState: existingNotificationState,
        });
      });
    });
  });

  describe("CLOSE_PDF", () => {
    it("can close a tab", () => {
      const existingTabsState = {
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [
          {
            documentId: "1",
            url: undefined,
          },
          {
            documentId: "2",
            url: undefined,
          },
        ],
      } as CombinedState["tabsState"];

      const nextState = reducer(
        {
          tabsState: existingTabsState,
        } as CombinedState,
        { type: "CLOSE_PDF", payload: { pdfId: "2" } }
      );

      expect(nextState.tabsState).toEqual({
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [
          {
            documentId: "1",
            url: undefined,
          },
        ],
      });
    });
  });

  describe("SET_ACTIVE_TAB", () => {
    it("can set an active tab", () => {
      const existingTabsState = {
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [
          {
            documentId: "1",
            url: undefined,
          },
          {
            documentId: "2",
            url: undefined,
          },
        ],
        activeTabId: "",
      } as CombinedState["tabsState"];

      const nextState = reducer(
        {
          tabsState: existingTabsState,
        } as CombinedState,
        { type: "SET_ACTIVE_TAB", payload: { pdfId: "2" } }
      );

      expect(nextState.tabsState).toEqual({
        headers: {
          Authorization: "foo",
          "Correlation-Id": "foo1",
        } as HeadersInit,
        items: [
          {
            documentId: "1",
            url: undefined,
          },
          {
            documentId: "2",
            url: undefined,
          },
        ],
        activeTabId: "2",
      });
    });
  });

  describe("UPDATE_SEARCH_TERM", () => {
    it("can update search term", () => {
      const existingState = {
        searchTerm: "foo",
        searchState: {
          submittedSearchTerm: "abc",
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_TERM",
        payload: { searchTerm: "bar" },
      });

      expect(nextState).toEqual({
        searchTerm: "bar",
        searchState: {
          lastSubmittedSearchTerm: "abc",
          submittedSearchTerm: "abc",
        },
      });
    });
  });

  describe("UPDATE_SEARCH_TYPE", () => {
    it("can update search type", () => {
      const existingState = {
        searchState: {
          searchType: "documentName",
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_TYPE",
        payload: "documentContent",
      });

      expect(nextState).toEqual({
        searchState: {
          searchType: "documentContent",
        },
      });
    });
  });

  describe("CLOSE_SEARCH_RESULTS", () => {
    it("can close search results", () => {
      const existingSearchState = {
        isResultsVisible: true,
      } as CombinedState["searchState"];

      const nextState = reducer(
        { searchState: existingSearchState } as CombinedState,
        { type: "CLOSE_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        isResultsVisible: false,
      });
    });
  });

  describe("LAUNCH_SEARCH_RESULTS", () => {
    beforeEach(() => {
      jest
        .spyOn(sanitizeSearchTerm, "sanitizeSearchTerm")
        .mockImplementation((input) => {
          return "bar";
        });
    });

    it("Should match the state when search for the first time", () => {
      const existingState = {
        documentsState: {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        },
        pipelineState: { status: "complete", data: {} },
        searchTerm: "bar",
        searchState: {
          isResultsVisible: false,
          searchConfigs: {
            documentName: {
              resultsOrder: "byDateDesc",
              results: { status: "succeeded" }
            }
          }
        },
      } as CombinedState;

      const mockUnsortedData = {} as MappedTextSearchResult;
      const mockData = {} as MappedTextSearchResult;
      const mockFilterOptions =
        {} as CombinedState["searchState"]["searchConfigs"]["documentName"]["filterOptions"];

      jest
        .spyOn(documentNameSearchMapper, "mapDocumentNameSearch")
        .mockImplementation((searchTerm, mappedCaseDocuments) => {
          if (
            searchTerm === existingState.searchTerm &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockUnsortedData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, resultOrder) => {
          if (
            mappedTextSearchResult === mockUnsortedData &&
            resultOrder === existingState.searchState.searchConfigs.documentName.resultsOrder
          ) {
            return mockData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(filters, "mapFilters")
        .mockImplementation((mappedTextSearchResult) => {
          if (mappedTextSearchResult === mockUnsortedData) {
            return mockFilterOptions;
          }
          throw new Error("Unexpected mock function arguments");
        });

      const nextState = reducer(
        existingState,
        { type: "LAUNCH_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        submittedSearchTerm: "bar",
        requestedSearchTerm: "bar",
        isResultsVisible: true,
        lastSubmittedSearchTerm: "",
        searchConfigs: {
          ...existingState.searchState.searchConfigs,
          documentName: {
            ...existingState.searchState.searchConfigs.documentName,
            filterOptions: mockFilterOptions,
            results: {
              status: "succeeded",
              data: mockData,
            },
          }
        }
      } as CombinedState["searchState"]);
    });
    it("Should match the state when search for any subsequent time", () => {
      jest
        .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
        .mockImplementation((a, b) => {
          return false;
        });

      const existingState = {
        documentsState: {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        },
        pipelineState: { status: "complete", data: {} },
        searchTerm: "bar",
        searchState: {
          isResultsVisible: false,
          requestedSearchTerm: "foo",
          submittedSearchTerm: "foo",
          lastSubmittedSearchTerm: "",
          searchConfigs: {
            documentName: {
              resultsOrder: "byDateDesc",
              results: { status: "succeeded" }
            }
          }
        },
      } as CombinedState;

      const mockUnsortedData = {} as MappedTextSearchResult;
      const mockData = {} as MappedTextSearchResult;
      const mockFilterOptions =
        {} as CombinedState["searchState"]["searchConfigs"]["documentName"]["filterOptions"];
      jest
        .spyOn(documentNameSearchMapper, "mapDocumentNameSearch")
        .mockImplementation((searchTerm, mappedCaseDocuments) => {
          if (
            searchTerm === existingState.searchTerm &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockUnsortedData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, resultOrder) => {
          if (
            mappedTextSearchResult === mockUnsortedData &&
            resultOrder === existingState.searchState.searchConfigs.documentName.resultsOrder
          ) {
            return mockData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(filters, "mapFilters")
        .mockImplementation((mappedTextSearchResult) => {
          if (mappedTextSearchResult === mockUnsortedData) {
            return mockFilterOptions;
          }
          throw new Error("Unexpected mock function arguments");
        });

      const nextState = reducer(
        {
          searchTerm: "abc",
          documentsState: existingState.documentsState,
          searchState: existingState.searchState,
        } as CombinedState,
        { type: "LAUNCH_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        submittedSearchTerm: "bar",
        requestedSearchTerm: "abc",
        isResultsVisible: true,
        lastSubmittedSearchTerm: "foo",
        searchConfigs: {
          ...existingState.searchState.searchConfigs,
          documentName: {
            ...existingState.searchState.searchConfigs.documentName,
            filterOptions: mockFilterOptions,
            results: {
              status: "succeeded",
              data: mockData,
            },
          }
        }
      } as CombinedState["searchState"]);
    });

    it("Should match the state when search if a pipelineRefresh is needed", () => {
      jest
        .spyOn(shouldTriggerPipelineRefresh, "shouldTriggerPipelineRefresh")
        .mockImplementation((a, b) => {
          return true;
        });

      const existingState = {
        documentsState: {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        },
        pipelineState: { status: "complete", data: {} },
        searchTerm: "bar",
        searchState: {
          isResultsVisible: false,
          requestedSearchTerm: "foo",
          submittedSearchTerm: "bar",
          lastSubmittedSearchTerm: "foo",
          searchConfigs: {
            documentName: {
              resultsOrder: "byDateDesc",
              results: { status: "succeeded" }
            }
          }
        },
      } as CombinedState;

      const mockUnsortedData = {} as MappedTextSearchResult;
      const mockData = {} as MappedTextSearchResult;
      const mockFilterOptions =
        {} as CombinedState["searchState"]["searchConfigs"]["documentName"]["filterOptions"];

      jest
        .spyOn(documentNameSearchMapper, "mapDocumentNameSearch")
        .mockImplementation((searchTerm, mappedCaseDocuments) => {
          if (
            searchTerm === existingState.searchTerm &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockUnsortedData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, resultOrder) => {
          if (
            mappedTextSearchResult === mockUnsortedData &&
            resultOrder === existingState.searchState.searchConfigs.documentName.resultsOrder
          ) {
            return mockData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(filters, "mapFilters")
        .mockImplementation((mappedTextSearchResult) => {
          if (mappedTextSearchResult === mockUnsortedData) {
            return mockFilterOptions;
          }
          throw new Error("Unexpected mock function arguments");
        });


      const nextState = reducer(
        {
          searchTerm: "abc",
          documentsState: existingState.documentsState,
          searchState: existingState.searchState,
        } as CombinedState,
        { type: "LAUNCH_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        submittedSearchTerm: "bar",
        requestedSearchTerm: "abc",
        isResultsVisible: true,
        lastSubmittedSearchTerm: "",
        searchConfigs: {
          ...existingState.searchState.searchConfigs,
          documentName: {
            ...existingState.searchState.searchConfigs.documentName,
            filterOptions: mockFilterOptions,
            results: {
              status: "succeeded",
              data: mockData,
            },
          }
        }

      } as CombinedState["searchState"]);
    });

    it("can trim spaces from the search term", () => {
      const existingState = {
        documentsState: {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        },
        pipelineState: { status: "complete", data: {} },
        searchTerm: "bar",
        searchState: {
          isResultsVisible: false,
          searchConfigs: {
            documentName: {
              resultsOrder: "byDateDesc",
              results: { status: "succeeded" }
            }
          }
        },
      } as CombinedState;

      const mockUnsortedData = {} as MappedTextSearchResult;
      const mockData = {} as MappedTextSearchResult;
      const mockFilterOptions =
        {} as CombinedState["searchState"]["searchConfigs"]["documentName"]["filterOptions"];
      jest
        .spyOn(documentNameSearchMapper, "mapDocumentNameSearch")
        .mockImplementation((searchTerm, mappedCaseDocuments) => {
          if (
            searchTerm === existingState.searchTerm &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockUnsortedData;
          }
          throw new Error("Unexpected mock function arguments");
        });
      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, resultOrder) => {
          if (
            mappedTextSearchResult === mockUnsortedData &&
            resultOrder === existingState.searchState.searchConfigs.documentName.resultsOrder
          ) {
            return mockData;
          }
          throw new Error("Unexpected mock function arguments");
        });
      jest
        .spyOn(filters, "mapFilters")
        .mockImplementation((mappedTextSearchResult) => {
          if (mappedTextSearchResult === mockUnsortedData) {
            return mockFilterOptions;
          }
          throw new Error("Unexpected mock function arguments");
        });


      const nextState = reducer(
        {
          searchTerm: " foo ",
          documentsState: existingState.documentsState,
          searchState: existingState.searchState,
        } as CombinedState,
        { type: "LAUNCH_SEARCH_RESULTS" }
      );

      expect(nextState.searchState).toEqual({
        submittedSearchTerm: "bar",
        requestedSearchTerm: "foo",
        isResultsVisible: true,
        lastSubmittedSearchTerm: "",
        searchConfigs: {
          ...existingState.searchState.searchConfigs,
          documentName: {
            ...existingState.searchState.searchConfigs.documentName,
            filterOptions: mockFilterOptions,
            results: {
              status: "succeeded",
              data: mockData,
            },
          }
        }
      } as CombinedState["searchState"]);
    });
  });

  describe("UPDATE_SEARCH_RESULTS", () => {
    it("throws if search call fails", () => {
      expect(() =>
        reducer({} as CombinedState, {
          type: "UPDATE_SEARCH_RESULTS",
          payload: {
            status: "failed",
            error: ERROR,
            httpStatusCode: undefined,
          },
        })
      ).toThrowError(ERROR);
    });

    it("returns a loading searchState if the search call is loading", () => {
      const nextState = reducer({
        searchState: {
          searchConfigs: {
            documentContent: { results: { status: "loading" } }
          }
        }
      } as CombinedState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "loading",
        },
      });

      expect(nextState).toEqual({
        searchState: {
          searchConfigs: {
            documentContent: { results: { status: "loading" } }
          }
        },
      });
    });

    it("does not alter state if documentState is not succeeded", () => {
      const existingState = {
        documentsState: { status: "loading" },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: [],
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("does not alter state if pipelineState is not succeeded", () => {
      const existingState = {
        documentsState: { status: "succeeded" },
        pipelineState: { status: "incomplete" },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: [],
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("does not alter state if there is no submitted search term", () => {
      const existingState = {
        documentsState: { status: "succeeded" },
        pipelineState: { status: "complete" },
        searchState: { submittedSearchTerm: "" },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: [],
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("does not alter state if the search call carries undefined data", () => {
      const existingState = {
        documentsState: { status: "succeeded" },
        pipelineState: { status: "complete" },
        searchState: {
          submittedSearchTerm: "foo", searchConfigs: {
            documentName: { results: { status: "succeeded" } }
          }
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: {
          status: "succeeded",
          data: undefined,
        } as ApiResult<undefined | ApiTextSearchResult[]>,
      });

      expect(nextState).toBe(existingState);
    });

    it("can update search results, update missing documents and build filter options", () => {
      const existingState = {
        documentsState: {
          status: "succeeded",
          data: [] as MappedCaseDocument[],
        },
        pipelineState: { status: "complete", data: {} },
        featureFlags: {
          documentNameSearch: false,
        },
        searchState: {
          submittedSearchTerm: "foo", searchConfigs: {
            documentName: {
              resultsOrder: "byDateDesc",
              results: { status: "succeeded", data: {} }
            },
            documentContent: {
              resultsOrder: "byDateDesc",
              results: { status: "succeeded" }
            }
          }
        },
      } as CombinedState;

      const inputPayload = {
        status: "succeeded",
        data: [],
      } as ApiResult<undefined | ApiTextSearchResult[]>;

      const mockFilteredApiResults = [] as ApiTextSearchResult[];

      const mockUnsortedData = {} as MappedTextSearchResult;
      const mockData = {} as MappedTextSearchResult;
      const mockMissingDocs = {} as CombinedState["searchState"]["missingDocs"];
      const mockFilterOptions =
        {} as CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"];

      jest
        .spyOn(filterApiResults, "filterApiResults")
        .mockImplementation((apiResults, existingDocuments) => {
          if (
            inputPayload.status === "succeeded" &&
            apiResults === inputPayload.data &&
            existingState.documentsState.status === "succeeded" &&
            existingDocuments === existingState.documentsState.data
          ) {
            return mockFilteredApiResults;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(textSearchMapper, "mapTextSearch")
        .mockImplementation((textSearchResult, mappedCaseDocuments) => {
          if (
            textSearchResult === mockFilteredApiResults &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockUnsortedData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(combineDocumentNameMatches, "combineDocumentNameMatches")
        .mockImplementation((mappedTextSearchResult, documentNameMatches, documentNameSearchFeatureEnabled) => {
          if (
            mappedTextSearchResult === mockUnsortedData &&
            existingState.documentsState.status === "succeeded" &&
            existingState.searchState.searchConfigs.documentName.results.status === "succeeded" &&
            documentNameMatches === existingState.searchState.searchConfigs.documentName.results.data.documentResults &&
            !documentNameSearchFeatureEnabled
          ) {
            return mockUnsortedData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, resultOrder) => {
          if (
            mappedTextSearchResult === mockUnsortedData &&
            resultOrder === existingState.searchState.searchConfigs.documentContent.resultsOrder
          ) {
            return mockData;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(missingDocuments, "mapMissingDocuments")
        .mockImplementation((pipelineResults, mappedCaseDocuments) => {
          if (
            pipelineResults === existingState.pipelineState.data &&
            existingState.documentsState.status === "succeeded" &&
            mappedCaseDocuments === existingState.documentsState.data
          ) {
            return mockMissingDocs;
          }
          throw new Error("Unexpected mock function arguments");
        });

      jest
        .spyOn(filters, "mapFilters")
        .mockImplementation((mappedTextSearchResult) => {
          if (mappedTextSearchResult === mockUnsortedData) {
            return mockFilterOptions;
          }
          throw new Error("Unexpected mock function arguments");
        });

      const nextState = reducer(existingState, {
        type: "UPDATE_SEARCH_RESULTS",
        payload: inputPayload,
      });

      expect(nextState).toEqual({
        ...existingState,
        searchState: {
          ...existingState.searchState,
          missingDocs: mockMissingDocs,
          searchConfigs: {
            ...existingState.searchState.searchConfigs,
            documentContent: {
              ...existingState.searchState.searchConfigs.documentName,
              filterOptions: mockFilterOptions,
              results: {
                status: "succeeded",
                data: mockData,
              },
            }
          }

        },
      });

      expect(nextState.searchState.missingDocs).toBe(mockMissingDocs);
      expect(nextState.searchState.searchConfigs.documentContent.filterOptions).toBe(mockFilterOptions);

      expect(
        nextState.searchState.searchConfigs.documentContent.results.status === "succeeded" &&
        nextState.searchState.searchConfigs.documentContent.results.data
      ).toBe(mockData);
    });
  });

  describe("CHANGE_RESULTS_ORDER", () => {
    it("can update the stored results order but not change search results ordering if the search state is still loading", () => {
      const existingState = {
        searchState: {
          searchType: "documentContent",
          searchConfigs: {
            documentContent: {
              resultsOrder: "byOccurancesPerDocumentDesc",
              results: { status: "loading" }
            }
          }
        },

      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "CHANGE_RESULTS_ORDER",
        payload: "byDateDesc",
      });

      expect(nextState).toEqual({
        searchState: {
          searchType: "documentContent",
          searchConfigs: {
            documentContent: {
              resultsOrder: "byDateDesc",
              results: { status: "loading" }
            }
          }
        },
      });
    });

    it("can update results ordering if search state is ready", () => {
      const existingMappedTextSearchResult = {} as MappedTextSearchResult;
      const expectedMappedTextSearchResult = {} as MappedTextSearchResult;

      jest
        .spyOn(sorter, "sortMappedTextSearchResult")
        .mockImplementation((mappedTextSearchResult, sortOrder) => {
          if (mappedTextSearchResult !== existingMappedTextSearchResult)
            throw new Error();
          if (sortOrder !== "byDateDesc") throw new Error();
          return expectedMappedTextSearchResult;
        });

      const existingState = {
        searchState: {
          searchType: "documentContent",
          searchConfigs: {
            documentContent: {
              resultsOrder: "byOccurancesPerDocumentDesc",
              results: { status: "succeeded", data: existingMappedTextSearchResult }
            }
          }
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "CHANGE_RESULTS_ORDER",
        payload: "byDateDesc",
      });

      expect(nextState).toEqual({
        searchState: {
          searchType: "documentContent",
          searchConfigs: {
            documentContent: {
              resultsOrder: "byDateDesc",
              results: { status: "succeeded", data: expectedMappedTextSearchResult }
            }
          }
        },
      });
    });
  });

  describe("UPDATE_FILTER", () => {
    it("can update filters but not sort data if search state is still loading", () => {
      const existingSearchState = {
        searchType: "documentContent",
        searchConfigs: {
          documentContent: {
            results: { status: "loading" },
            filterOptions: {
              category: {
                a: { label: "", count: -1, isSelected: true },
                b: { label: "", count: -1, isSelected: false },
              },
              docType: {
                a: { label: "", count: -1, isSelected: true },
                b: { label: "", count: -1, isSelected: true },
              },
            } as CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"]
          }
        },
      } as CombinedState["searchState"];

      const result = reducer(
        { searchState: existingSearchState } as CombinedState,
        {
          type: "UPDATE_FILTER",
          payload: { filter: "docType", id: "b", isSelected: true },
        }
      );

      expect(result).toEqual({
        searchState: {
          searchType: "documentContent",
          searchConfigs: {
            documentContent: {
              filterOptions: {
                category: {
                  a: {
                    count: -1,
                    isSelected: true,
                    label: "",
                  },
                  b: {
                    count: -1,
                    isSelected: false,
                    label: "",
                  },
                },
                docType: {
                  a: {
                    count: -1,
                    isSelected: true,
                    label: "",
                  },
                  b: {
                    count: -1,
                    isSelected: true,
                    label: "",
                  },
                },
              },
              results: { status: "loading" }
            }
          }
        },
      });
    });

    it("can update filters and sort data if search state has succeeded", () => {
      jest
        .spyOn(documentVisibility, "isDocumentVisible")
        .mockImplementation(({ documentId }, filterOptions) => {
          switch (documentId) {
            case "1":
              return { isVisible: true, hasChanged: true };
            case "2":
              return { isVisible: false, hasChanged: true };
            case "3":
              return { isVisible: true, hasChanged: false };
            default:
              throw new Error("Unexpected mock function arguments");
          }
        });

      const existingSearchState = {
        searchType: "documentContent",
        searchConfigs: {
          documentContent: {
            results: {
              status: "succeeded", data: {
                documentResults: [
                  { documentId: "1", occurrencesInDocumentCount: 2 },
                  { documentId: "2", occurrencesInDocumentCount: 3 },
                  { documentId: "3", occurrencesInDocumentCount: 7 },
                ] as MappedDocumentResult[],
              }
            },
            filterOptions: {
              category: {},
              docType: {},
            } as CombinedState["searchState"]["searchConfigs"]["documentContent"]["filterOptions"],
          }
        }
      } as CombinedState["searchState"];

      const result = reducer(
        { searchState: existingSearchState } as CombinedState,
        {
          type: "UPDATE_FILTER",
          payload: { filter: "docType", id: "a", isSelected: true },
        }
      );

      expect(result).toEqual({
        searchState: {
          searchType: "documentContent",
          searchConfigs: {
            documentContent: {
              results: {
                data: {
                  documentResults: [
                    {
                      documentId: "1",
                      isVisible: true,
                      occurrencesInDocumentCount: 2,
                    },
                    {
                      documentId: "2",
                      isVisible: false,
                      occurrencesInDocumentCount: 3,
                    },
                    { documentId: "3", occurrencesInDocumentCount: 7 },
                  ],
                  filteredDocumentCount: 1,
                  filteredOccurrencesCount: 2,
                },
                status: "succeeded",
              },
              filterOptions: {
                category: {},
                docType: {
                  a: { isSelected: true },
                },
              },
            },
          },
        },
      });

      // assert we have been given the same reference to the object if
      //  the document has not changed
      expect(
        result.searchState.searchConfigs.documentContent.results.status === "succeeded" &&
        result.searchState.searchConfigs.documentContent.results.data.documentResults[2]
      ).toBe(
        existingSearchState.searchConfigs.documentContent.results.status === "succeeded" &&
        existingSearchState.searchConfigs.documentContent.results.data.documentResults[2]
      );
    });
  });

  describe("ADD_REDACTION", () => {
    it("can add a redaction", () => {
      jest.useFakeTimers().setSystemTime(new Date("2022-01-01"));

      const existingTabsState = {
        items: [
          {
            documentId: "1",
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 2 },
                id: "1640995199999",
              },
            ] as IPdfHighlight[],
          },
          { documentId: "bar", redactionHighlights: [] as IPdfHighlight[] },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "ADD_REDACTION",
          payload: {
            documentId: "1",
            redactions: [
              {
                type: "redaction",
                position: { pageNumber: 1 },
              },
            ] as NewPdfHighlight[],
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              redactionHighlights: [
                {
                  type: "redaction",
                  position: { pageNumber: 2 },
                  id: "1640995199999",
                },
                {
                  type: "redaction",
                  position: { pageNumber: 1 },
                  id: "1640995200000-0",
                },
              ],
            },
            { documentId: "bar", redactionHighlights: [] },
          ],
        },
      });
    });
  });

  describe("REMOVE_REDACTION", () => {
    it("can remove a redaction", () => {
      const existingTabsState = {
        items: [
          {
            documentId: "1",
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 2 },
                id: "1640995199999",
              },
              {
                type: "redaction",
                position: { pageNumber: 1 },
                id: "1640995200000",
              },
            ] as IPdfHighlight[],
          },
          { documentId: "bar", redactionHighlights: [] as IPdfHighlight[] },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "REMOVE_REDACTION",
          payload: {
            documentId: "1",
            redactionId: "1640995200000",
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              redactionHighlights: [
                {
                  type: "redaction",
                  position: { pageNumber: 2 },
                  id: "1640995199999",
                },
              ],
            },
            { documentId: "bar", redactionHighlights: [] },
          ],
        },
      });
    });
  });

  describe("REMOVE_PAGE_DELETE_REDACTION", () => {
    it("can remove a page delete redaction", () => {
      const existingTabsState = {
        items: [
          {
            documentId: "1",
            pageDeleteRedactions: [
              { id: "1_1" },
              { id: "1_2" },
            ] as IPageDeleteRedaction[],
          },
          {
            documentId: "2",
            pageDeleteRedactions: [{ id: "2_1" }] as IPageDeleteRedaction[],
          },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "REMOVE_PAGE_DELETE_REDACTION",
          payload: {
            documentId: "1",
            redactionId: "1_2",
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              pageDeleteRedactions: [{ id: "1_1" }] as IPageDeleteRedaction[],
            },
            {
              documentId: "2",
              pageDeleteRedactions: [{ id: "2_1" }] as IPageDeleteRedaction[],
            },
          ],
        },
      });
    });
  });

  describe("REMOVE_ALL_REDACTIONS", () => {
    it("can remove all redactions", () => {
      jest.useFakeTimers().setSystemTime(new Date("2022-01-01"));

      const existingTabsState = {
        items: [
          {
            documentId: "1",
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 2 },
                id: "1640995199999",
              },
              {
                type: "redaction",
                position: { pageNumber: 1 },
                id: "1640995200000",
              },
            ] as IPdfHighlight[],
          },
          {
            documentId: "2",
            redactionHighlights: [
              {
                type: "redaction",
                position: { pageNumber: 3 },
                id: "1640995199998",
              },
            ] as IPdfHighlight[],
          },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "REMOVE_ALL_REDACTIONS",
          payload: {
            documentId: "1",
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              redactionHighlights: [],
              pageDeleteRedactions: [],
            },
            {
              documentId: "2",
              redactionHighlights: [
                {
                  type: "redaction",
                  position: { pageNumber: 3 },
                  id: "1640995199998",
                },
              ],
            },
          ],
        },
      });
    });
  });

  describe("UPDATE_DOCUMENT_LOCK_STATE", () => {
    it("can update document locked state", () => {
      const existingTabsState = {
        items: [
          {
            documentId: "1",
            clientLockedState: "unlocked",
          },
          {
            documentId: "2",
            clientLockedState: "unlocking",
          },
        ],
      } as CombinedState["tabsState"];

      const result = reducer(
        { tabsState: existingTabsState } as CombinedState,
        {
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: {
            documentId: "1",
            lockedState: "locked",
          },
        }
      );

      expect(result).toEqual({
        tabsState: {
          items: [
            { documentId: "1", clientLockedState: "locked" },
            { documentId: "2", clientLockedState: "unlocking" },
          ],
        },
      });
    });
  });

  describe("UPDATE_DOCUMENT_REFRESH", () => {
    it("can update documentRefreshData", () => {
      const existingState = {
        documentRefreshData: {
          startDocumentRefresh: false,
          savedDocumentDetails: [{ documentId: "1", versionId: 1 }],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_DOCUMENT_REFRESH",
        payload: {
          startDocumentRefresh: true,
          savedDocumentDetails: {
            documentId: "2",
            versionId: 1,
          },
        },
      });

      expect(result).toEqual({
        documentRefreshData: {
          startDocumentRefresh: true,
          savedDocumentDetails: [
            { documentId: "1", versionId: 1 },
            { documentId: "2", versionId: 1 },
          ],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      });
    });
    it("can update documentRefreshData if the payload doesn't have savedDocumentDetails ", () => {
      const existingState = {
        documentRefreshData: {
          startDocumentRefresh: false,
          savedDocumentDetails: [{ documentId: "1", versionId: 1 }],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_DOCUMENT_REFRESH",
        payload: {
          startDocumentRefresh: true,
        },
      });

      expect(result).toEqual({
        documentRefreshData: {
          startDocumentRefresh: true,
          savedDocumentDetails: [{ documentId: "1", versionId: 1 }],
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
        },
      });
    });
  });

  describe("UPDATE_PIPELINE_REFRESH", () => {
    it("can update pipelineRefreshData", () => {
      const existingState = {
        pipelineRefreshData: {
          startPipelineRefresh: false,
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
          localLastRefreshTime: "2023-04-05T15:02:17.601Z",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_PIPELINE_REFRESH",
        payload: {
          startPipelineRefresh: true,
        },
      });

      expect(result).toEqual({
        pipelineRefreshData: {
          startPipelineRefresh: true,
          lastProcessingCompleted: "2023-04-05T15:02:17.601Z",
          localLastRefreshTime: "2023-04-05T15:02:17.601Z",
        },
      });
    });
  });
  describe("UPDATE_CONVERSION_STATUS", () => {
    it("can update conversion status of already existing document in the localDocumentState", () => {
      const existingState = {
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_CONVERSION_STATUS",
        payload: {
          documentId: "1",
          status: "EncryptionOrPasswordProtection",
        },
      });

      expect(result).toEqual({
        localDocumentState: {
          "1": { conversionStatus: "EncryptionOrPasswordProtection" },
        },
      });
    });
    it("can add new documents with conversion status in the localDocumentState", () => {
      const existingState = {
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_CONVERSION_STATUS",
        payload: {
          documentId: "2",
          status: "EncryptionOrPasswordProtection",
        },
      });

      expect(result).toEqual({
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
          "2": { conversionStatus: "EncryptionOrPasswordProtection" },
        },
      });
    });
  });

  describe("SHOW_ERROR_MODAL", () => {
    it("can show the error modal", () => {
      const existingState = {
        errorModal: {
          show: false,
          message: "",
          title: "",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "SHOW_ERROR_MODAL",
        payload: {
          type: "saveredaction",
          message: "error message",
          title: "error title",
        },
      });

      expect(result).toEqual({
        errorModal: {
          show: true,
          message: "error message",
          title: "error title",
          type: "saveredaction",
        },
      });
    });
  });
  describe("HIDE_ERROR_MODAL", () => {
    it("can hide the error modal", () => {
      const existingState = {
        errorModal: {
          show: true,
          message: "error message",
          title: "error title",
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "HIDE_ERROR_MODAL",
      });

      expect(result).toEqual({
        errorModal: {
          show: false,
          message: "",
          title: "",
          type: "",
        },
      });
    });
  });

  describe("SHOW_HIDE_DOCUMENT_ISSUE_MODAL", () => {
    it("Should update the documentIssueModal state", () => {
      const existingState = {} as CombinedState;

      const nextState = reducer(existingState, {
        type: "SHOW_HIDE_DOCUMENT_ISSUE_MODAL",
        payload: true,
      });

      expect(nextState).toEqual({ documentIssueModal: { show: true } });
    });
  });

  describe("SHOW_REDACTION_LOG_MODAL", () => {
    it("Should update the redaction log state", () => {
      const existingState = {} as CombinedState;
      const nextState = reducer(existingState, {
        type: "SHOW_REDACTION_LOG_MODAL",
        payload: {
          type: "UNDER" as any,
          savedRedactionTypes: [
            { id: "1", name: "Occupation" },
            { id: "2", name: "Occupation" },
          ],
        },
      });

      expect(nextState).toEqual({
        redactionLog: {
          showModal: true,
          type: "UNDER" as any,
          savedRedactionTypes: [
            { id: "1", name: "Occupation" },
            { id: "2", name: "Occupation" },
          ],
        },
      });
    });
  });

  describe("HIDE_REDACTION_LOG_MODAL", () => {
    it("Should update the redaction log state", () => {
      const existingState = {
        redactionLog: {
          showModal: true,
          type: "UNDER" as any,
          savedRedactionTypes: [
            { id: "1", name: "Occupation" },
            { id: "2", name: "Occupation" },
          ],
        },
      } as CombinedState;
      const nextState = reducer(existingState, {
        type: "HIDE_REDACTION_LOG_MODAL",
      });

      expect(nextState).toEqual({
        redactionLog: {
          type: "UNDER",
          showModal: false,
          savedRedactionTypes: [],
        },
      });
    });
  });

  describe("UPDATE_FEATURE_FLAGS_DATA", () => {
    it("Should update the redaction log state", () => {
      const existingState = {} as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_FEATURE_FLAGS_DATA",
        payload: {
          redactionLog: true,
          fullScreen: false,
          notes: true,
          searchPII: false,
          renameDocument: true,
        } as any,
      });

      expect(nextState).toEqual({
        featureFlags: {
          redactionLog: true,
          fullScreen: false,
          notes: true,
          searchPII: false,
          renameDocument: true,
        },
      });
    });
  });

  describe("ENABLE_AREA_REDACTION_MODE", () => {
    it("Should update the tabstate areaOnlyRedactionMode correctly for the document", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              areaOnlyRedactionMode: false,
            },
            {
              documentId: "2",
              areaOnlyRedactionMode: true,
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "ENABLE_AREA_REDACTION_MODE",
        payload: {
          documentId: "1",
          enableAreaOnlyMode: true,
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              areaOnlyRedactionMode: true,
            },
            {
              documentId: "2",
              areaOnlyRedactionMode: true,
            },
          ],
        },
      });

      const nextState2 = reducer(existingState, {
        type: "ENABLE_AREA_REDACTION_MODE",
        payload: {
          documentId: "2",
          enableAreaOnlyMode: false,
        } as any,
      });
      expect(nextState2).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              areaOnlyRedactionMode: false,
            },
            {
              documentId: "2",
              areaOnlyRedactionMode: false,
            },
          ],
        },
      });
    });
  });

  describe("UPDATE_STORED_USER_DATA", () => {
    it("Should update the redaction log state", () => {
      const existingState = {} as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_STORED_USER_DATA",
        payload: {
          storedUserData: {
            readUnread: ["1", "2"],
          },
        },
      } as any);

      expect(nextState).toEqual({
        storedUserData: {
          status: "succeeded",
          data: { readUnread: ["1", "2"] },
        },
      });
    });
  });

  describe("UPDATE_NOTES_DATA", () => {
    it("Should update the add note status correctly on saving, success and failure", () => {
      const existingState = {
        notes: [
          {
            documentId: "1",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
        ],
      } as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_NOTES_DATA",
        payload: {
          documentId: "1",
          addNoteStatus: "saving",
          getNoteStatus: "initial",
        },
      } as any);

      expect(nextState).toEqual({
        notes: [
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "1",
            addNoteStatus: "saving",
            getNoteStatus: "initial",
          },
        ],
      });
      const nextState2 = reducer(existingState, {
        type: "UPDATE_NOTES_DATA",
        payload: {
          documentId: "1",
          addNoteStatus: "failure",
          getNoteStatus: "initial",
        },
      } as any);

      expect(nextState2).toEqual({
        notes: [
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "1",
            addNoteStatus: "failure",
            getNoteStatus: "initial",
          },
        ],
      });
      const nextState3 = reducer(existingState, {
        type: "UPDATE_NOTES_DATA",
        payload: {
          documentId: "1",
          addNoteStatus: "success",
          getNoteStatus: "initial",
        },
      } as any);

      expect(nextState3).toEqual({
        notes: [
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "1",
            addNoteStatus: "success",
            getNoteStatus: "initial",
          },
        ],
      });
    });

    it("Should update the notes data correctly", () => {
      const existingState = {
        notes: [
          {
            documentId: "1",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
        ],
      } as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_NOTES_DATA",
        payload: {
          documentId: "1",
          addNoteStatus: "initial",
          getNoteStatus: "success",
          notesData: [{ id: "note_1", createdByName: "abc" }],
        },
      } as any);

      expect(nextState).toEqual({
        notes: [
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "1",
            addNoteStatus: "initial",
            getNoteStatus: "success",
            notes: [{ id: "note_1", createdByName: "abc" }],
          },
        ],
      });
      const nextState2 = reducer(existingState, {
        type: "UPDATE_NOTES_DATA",
        payload: {
          documentId: "1",
          addNoteStatus: "initial",
          getNoteStatus: "loading",
          notesData: [],
        },
      } as any);

      expect(nextState2).toEqual({
        notes: [
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "1",
            addNoteStatus: "initial",
            getNoteStatus: "loading",
            notes: [],
          },
        ],
      });
      const nextState3 = reducer(existingState, {
        type: "UPDATE_NOTES_DATA",
        payload: {
          documentId: "1",
          addNoteStatus: "initial",
          getNoteStatus: "failure",
          notesData: [],
        },
      } as any);

      expect(nextState3).toEqual({
        notes: [
          {
            documentId: "2",
            addNoteStatus: "initial",
            getNoteStatus: "initial",
          },
          {
            documentId: "1",
            addNoteStatus: "initial",
            getNoteStatus: "failure",
            notes: [],
          },
        ],
      });
    });
  });

  describe("UPDATE_RENAME_DATA", () => {
    it("It should update renameDocuments with correct properties when the saveRenameStatus is failure or success", () => {
      const existingState = {
        renameDocuments: [
          {
            documentId: "1",
            newName: "abc",
            saveRenameStatus: "saving",
            saveRenameRefreshStatus: "initial",
          },
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
        ],
      } as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_RENAME_DATA",
        payload: {
          properties: {
            documentId: "1",
            saveRenameStatus: "success",
            saveRenameRefreshStatus: "updating",
          },
        },
      });

      expect(nextState).toEqual({
        renameDocuments: [
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
          {
            documentId: "1",
            newName: "abc",
            saveRenameStatus: "success",
            saveRenameRefreshStatus: "updating",
          },
        ],
      });

      const nextState2 = reducer(existingState, {
        type: "UPDATE_RENAME_DATA",
        payload: {
          properties: {
            documentId: "1",
            saveRenameStatus: "failure",
          },
        },
      });

      expect(nextState2).toEqual({
        renameDocuments: [
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
          {
            documentId: "1",
            newName: "abc",
            saveRenameStatus: "failure",
            saveRenameRefreshStatus: "initial",
          },
        ],
      });
    });
    it("It should update renameDocuments with correct properties when the saveRenameRefreshStatus is updating or updated", () => {
      const existingState = {
        renameDocuments: [
          {
            documentId: "1",
            newName: "abc",
            saveRenameStatus: "success",
            saveRenameRefreshStatus: "initial",
          },
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
        ],
      } as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_RENAME_DATA",
        payload: {
          properties: {
            documentId: "1",
            saveRenameRefreshStatus: "updating",
          },
        },
      });

      expect(nextState).toEqual({
        renameDocuments: [
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
          {
            documentId: "1",
            newName: "abc",
            saveRenameStatus: "success",
            saveRenameRefreshStatus: "updating",
          },
        ],
      });

      const nextState2 = reducer(existingState, {
        type: "UPDATE_RENAME_DATA",
        payload: {
          properties: {
            documentId: "1",
            saveRenameRefreshStatus: "updated",
          },
        },
      });

      expect(nextState2).toEqual({
        renameDocuments: [
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
          {
            documentId: "1",
            newName: "abc",
            saveRenameStatus: "success",
            saveRenameRefreshStatus: "updated",
          },
        ],
      });
    });
    it("It should update renameDocuments with correct properties when the saveRenameStatus is saving", () => {
      const existingState = {
        renameDocuments: [
          {
            documentId: "1",
            newName: "abc",
            saveRenameStatus: "success",
            saveRenameRefreshStatus: "updated",
          },
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
        ],
      } as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_RENAME_DATA",
        payload: {
          properties: {
            documentId: "1",
            newName: "abc_1",
            saveRenameStatus: "saving",
            saveRenameRefreshStatus: "initial",
          },
        },
      });

      expect(nextState).toEqual({
        renameDocuments: [
          {
            documentId: "2",
            newName: "abc",
            saveRenameStatus: "initial",
            saveRenameRefreshStatus: "initial",
          },
          {
            documentId: "1",
            newName: "abc_1",
            saveRenameStatus: "saving",
            saveRenameRefreshStatus: "initial",
          },
        ],
      });
    });
  });

  describe("UPDATE_RECLASSIFY_DATA", () => {
    it("Should correctly update reclassifyDocuments state when saveReclassifyRefreshStatus is initial or updating", () => {
      const existingState = {
        reclassifyDocuments: [
          {
            documentId: "1",
            newDocTypeId: 123,
            reclassified: true,
            saveReclassifyRefreshStatus: "initial",
          },
          {
            documentId: "2",
            newDocTypeId: 124,
            reclassified: true,
            saveReclassifyRefreshStatus: "updated",
          },
        ],
      } as CombinedState;
      const nextState = reducer(existingState, {
        type: "UPDATE_RECLASSIFY_DATA",
        payload: {
          properties: {
            documentId: "3",
            saveReclassifyRefreshStatus: "initial",
          },
        },
      });

      expect(nextState).toEqual({
        reclassifyDocuments: [
          {
            documentId: "1",
            newDocTypeId: 123,
            reclassified: true,
            saveReclassifyRefreshStatus: "initial",
          },
          {
            documentId: "2",
            newDocTypeId: 124,
            reclassified: true,
            saveReclassifyRefreshStatus: "updated",
          },
          {
            documentId: "3",
            saveReclassifyRefreshStatus: "initial",
          },
        ],
      });
      const nextState2 = reducer(existingState, {
        type: "UPDATE_RECLASSIFY_DATA",
        payload: {
          properties: {
            documentId: "1",
            saveReclassifyRefreshStatus: "updated",
          },
        },
      });

      expect(nextState2).toEqual({
        reclassifyDocuments: [
          {
            documentId: "2",
            newDocTypeId: 124,
            reclassified: true,
            saveReclassifyRefreshStatus: "updated",
          },
          {
            documentId: "1",
            newDocTypeId: 123,
            reclassified: true,
            saveReclassifyRefreshStatus: "updated",
          },
        ],
      });
    });
    it("Should correctly update reclassifyDocuments state when saveReclassifyRefreshStatus is updating", () => {
      const existingState = {
        reclassifyDocuments: [
          {
            documentId: "1",
            newDocTypeId: 123,
            reclassified: true,
            saveReclassifyRefreshStatus: "initial",
          },
          {
            documentId: "2",
            newDocTypeId: 124,
            reclassified: true,
            saveReclassifyRefreshStatus: "updated",
          },
        ],
      } as CombinedState;
      const nextState3 = reducer(existingState, {
        type: "UPDATE_RECLASSIFY_DATA",
        payload: {
          properties: {
            documentId: "1",
            newDocTypeId: 124,
            reclassified: true,
            saveReclassifyRefreshStatus: "updating",
          },
        },
      });
      expect(nextState3).toEqual({
        reclassifyDocuments: [
          {
            documentId: "2",
            newDocTypeId: 124,
            reclassified: true,
            saveReclassifyRefreshStatus: "updated",
          },
          {
            documentId: "1",
            newDocTypeId: 124,
            reclassified: true,
            saveReclassifyRefreshStatus: "updating",
          },
        ],
      });
    });
  });

  describe("SHOW_HIDE_PAGE_DELETION", () => {
    it("should update correctly when the deletePageMode is set to true", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: false,
              rotatePageMode: true,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "SHOW_HIDE_PAGE_DELETION",
        payload: {
          documentId: "1",
          deletePageMode: true,
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: true,
              rotatePageMode: false,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      });
    });
    it("should update correctly when the deletePageMode is set to false", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: true,
              rotatePageMode: true,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "SHOW_HIDE_PAGE_DELETION",
        payload: {
          documentId: "1",
          deletePageMode: false,
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: false,
              rotatePageMode: true,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      });
    });
  });

  describe("SHOW_HIDE_PAGE_ROTATION", () => {
    it("should update correctly when the rotatePageMode is set to true", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: true,
              rotatePageMode: false,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "SHOW_HIDE_PAGE_ROTATION",
        payload: {
          documentId: "1",
          rotatePageMode: true,
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: true,
              rotatePageMode: true,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      });
    });
    it("should update correctly when the rotatePageMode is set to false", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: true,
              rotatePageMode: true,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "SHOW_HIDE_PAGE_ROTATION",
        payload: {
          documentId: "1",
          rotatePageMode: false,
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              deletePageMode: true,
              rotatePageMode: false,
            },
            {
              documentId: "2",
              deletePageMode: false,
              rotatePageMode: true,
            },
          ],
        },
      });
    });
  });

  describe("ADD_PAGE_ROTATION", () => {
    it("should update correctly rotation angle of an already page existing rotation", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [{ id: "12", pageNumber: 1, rotationAngle: 0 }],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "ADD_PAGE_ROTATION",
        payload: {
          documentId: "1",
          pageRotations: [{ pageNumber: 1, rotationAngle: 90 }],
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [{ id: "12", pageNumber: 1, rotationAngle: 90 }],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      });
    });
    it("should update correctly rotation angle of a new page rotation", () => {
      const mockDate = new Date(123);
      jest.spyOn(global, "Date").mockImplementation(() => mockDate as any);
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [{ id: "12", pageNumber: 1, rotationAngle: 0 }],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "ADD_PAGE_ROTATION",
        payload: {
          documentId: "1",
          pageRotations: [{ pageNumber: 3, rotationAngle: 180 }],
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [
                { id: "12", pageNumber: 1, rotationAngle: 0 },
                { id: "123-0", pageNumber: 3, rotationAngle: 180 },
              ],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      });
      jest.spyOn(global, "Date").mockRestore();
    });
  });

  describe("REMOVE_PAGE_ROTATION", () => {
    it("should successfully remove the last page rotation available for that document", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [{ id: "12", pageNumber: 1, rotationAngle: 0 }],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "REMOVE_PAGE_ROTATION",
        payload: {
          documentId: "1",
          rotationId: "12",
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      });
    });
    it("should successfully remove the page rotation", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [
                { id: "12", pageNumber: 1, rotationAngle: 0 },
                { id: "13", pageNumber: 2, rotationAngle: 90 },
              ],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "REMOVE_PAGE_ROTATION",
        payload: {
          documentId: "1",
          rotationId: "13",
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [{ id: "12", pageNumber: 1, rotationAngle: 0 }],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      });
    });
  });

  describe("REMOVE_ALL_ROTATIONS", () => {
    it("should successfully remove all the page rotations for a document", () => {
      const existingState = {
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [
                { id: "12", pageNumber: 1, rotationAngle: 0 },
                { id: "123-0", pageNumber: 3, rotationAngle: 180 },
              ],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "REMOVE_ALL_ROTATIONS",
        payload: {
          documentId: "1",
        } as any,
      });

      expect(nextState).toEqual({
        tabsState: {
          items: [
            {
              documentId: "1",
              pageRotations: [],
            },
            {
              documentId: "2",
              pageRotations: [{ id: "13", pageNumber: 2, rotationAngle: 0 }],
            },
          ],
        },
      });
    });
  });

  describe("ACCORDION_OPEN_CLOSE", () => {
    it("should return the state without any update", () => {
      const existingState = {
        accordionState: {
          status: "loading",
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "ACCORDION_OPEN_CLOSE",
        payload: {
          id: "foo",
          open: true,
        } as any,
      });

      expect(nextState).toEqual({
        accordionState: {
          status: "loading",
        },
      });
    });
    it("Should open a section, leaving a mix of open and closed sections ", () => {
      const existingState = {
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: false, bar: false },
            isAllOpen: false,
            sections: [],
          },
        },
      } as unknown as CombinedState;

      const nextState = reducer(existingState, {
        type: "ACCORDION_OPEN_CLOSE",
        payload: {
          id: "foo",
          open: true,
        } as any,
      });
      expect(nextState).toEqual({
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: true, bar: false },
            isAllOpen: false,
            sections: [],
          },
        },
      });
    });
    it("Should open a section, leaving all sections opened if that is the last section to open", () => {
      const existingState = {
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: false, bar: true },
            isAllOpen: false,
            sections: [],
          },
        },
      } as unknown as CombinedState;

      const nextState = reducer(existingState, {
        type: "ACCORDION_OPEN_CLOSE",
        payload: {
          id: "foo",
          open: true,
        } as any,
      });
      expect(nextState).toEqual({
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: true, bar: true },
            isAllOpen: true,
            sections: [],
          },
        },
      });
    });
    it("Should close a section, leaving a mix of open and closed sections ", () => {
      const existingState = {
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: true, bar: true },
            isAllOpen: true,
            sections: [],
          },
        },
      } as unknown as CombinedState;

      const nextState = reducer(existingState, {
        type: "ACCORDION_OPEN_CLOSE",
        payload: {
          id: "foo",
          open: false,
        } as any,
      });
      expect(nextState).toEqual({
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: false, bar: true },
            isAllOpen: false,
            sections: [],
          },
        },
      });
    });
  });

  describe("ACCORDION_OPEN_CLOSE_ALL", () => {
    it("should return the state without any update", () => {
      const existingState = {
        accordionState: {
          status: "loading",
        },
      } as CombinedState;

      const nextState = reducer(existingState, {
        type: "ACCORDION_OPEN_CLOSE_ALL",
        payload: true,
      });

      expect(nextState).toEqual({
        accordionState: {
          status: "loading",
        },
      });
    });
    it("Should open all section", () => {
      const existingState = {
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: false, bar: false },
            isAllOpen: false,
            sections: [],
          },
        },
      } as unknown as CombinedState;

      const nextState = reducer(existingState, {
        type: "ACCORDION_OPEN_CLOSE_ALL",
        payload: true,
      });
      expect(nextState).toEqual({
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: true, bar: true },
            isAllOpen: true,
            sections: [],
          },
        },
      });
    });
    it("Should close all sections", () => {
      const existingState = {
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: true, bar: true },
            isAllOpen: true,
            sections: [],
          },
        },
      } as unknown as CombinedState;

      const nextState = reducer(existingState, {
        type: "ACCORDION_OPEN_CLOSE_ALL",
        payload: false,
      });
      expect(nextState).toEqual({
        accordionState: {
          status: "succeeded",
          data: {
            sectionsOpenStatus: { foo: false, bar: false },
            isAllOpen: false,
            sections: [],
          },
        },
      });
    });
  });

  describe("UPDATE_CONVERSION_STATUS", () => {
    it("can update conversion status of already existing document in the localDocumentState", () => {
      const existingState = {
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_CONVERSION_STATUS",
        payload: {
          documentId: "1",
          status: "EncryptionOrPasswordProtection",
        },
      });

      expect(result).toEqual({
        localDocumentState: {
          "1": { conversionStatus: "EncryptionOrPasswordProtection" },
        },
      });
    });
    it("can add new documents with conversion status in the localDocumentState", () => {
      const existingState = {
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
        },
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "UPDATE_CONVERSION_STATUS",
        payload: {
          documentId: "2",
          status: "EncryptionOrPasswordProtection",
        },
      });

      expect(result).toEqual({
        localDocumentState: {
          "1": { conversionStatus: "DocumentConverted" },
          "2": { conversionStatus: "EncryptionOrPasswordProtection" },
        },
      });
    });
  });

  describe("Notifications", () => {
    it("can register a notification to be ignored", () => {
      const existingState = {
        notificationState: {
          ignoreNextEvents: [],
          events: [],
        } as NotificationState,
      } as unknown as CombinedState;

      const result = reducer(existingState, {
        type: "REGISTER_NOTIFIABLE_EVENT",
        payload: { documentId: "1", reason: "New" },
      });

      expect(result).toEqual({
        notificationState: {
          ignoreNextEvents: [{ documentId: "1", reason: "New" }],
          events: [],
        } as NotificationState,
      });
    });

    describe("notification action handlers", () => {
      const priorNotificationState: NotificationState = {
        ignoreNextEvents: [],
        events: [],
      };

      const priorDocumentsState: AsyncResult<MappedCaseDocument[]> = {
        status: "succeeded",
        data: [],
      };

      const priorExistingState = {
        notificationState: priorNotificationState,
        documentsState: priorDocumentsState,
      } as unknown as CombinedState;

      const expectedNotificationState: NotificationState = {
        ignoreNextEvents: [],
        events: [],
      };

      const expectedDocumentsState: AsyncResult<MappedCaseDocument[]> = {
        status: "succeeded",
        data: [],
      };

      const expectedAccordionState: AsyncResult<AccordionData> = {
        status: "succeeded",
        data: { sectionsOpenStatus: {}, isAllOpen: false, sections: [] },
      };

      const badNotificationState: NotificationState = {
        ignoreNextEvents: [],
        events: [],
      };

      const badDocumentsState: AsyncResult<MappedCaseDocument[]> = {
        status: "succeeded",
        data: [],
      };

      const badAccordionState: AsyncResult<AccordionData> = {
        status: "succeeded",
        data: { sectionsOpenStatus: {}, isAllOpen: false, sections: [] },
      };
      beforeEach(() => {
        jest
          .spyOn(
            mapNotificationToDocumentsState,
            "mapNotificationToDocumentsState"
          )
          .mockImplementation(
            (incomingNotificationsState, incomingDocumentsState) =>
              incomingNotificationsState === expectedNotificationState &&
                incomingDocumentsState === priorDocumentsState
                ? expectedDocumentsState
                : badDocumentsState
          );

        jest
          .spyOn(accordionMapper, "mapAccordionState")
          .mockImplementation((incomingDocumentsState, oldState) =>
            incomingDocumentsState === expectedDocumentsState
              ? expectedAccordionState
              : badAccordionState
          );
      });

      it("should delegate clearing all notifications to a function owned by the notifications code", () => {
        jest
          .spyOn(notificationsMappingFunctions, "clearAllNotifications")
          .mockImplementation((incomingNotificationsState) =>
            incomingNotificationsState === priorNotificationState
              ? expectedNotificationState
              : badNotificationState
          );

        const result = reducer(priorExistingState, {
          type: "CLEAR_ALL_NOTIFICATIONS",
        });

        expect(result.notificationState).toBe(expectedNotificationState);
        expect(result.documentsState).toBe(expectedDocumentsState);
      });

      it("should delegate clearing a notification to a function owned by the notifications code", () => {
        const notificationId = 1;

        jest
          .spyOn(notificationsMappingFunctions, "clearNotification")
          .mockImplementation(
            (incomingNotificationsState, incomingNotificationId) =>
              incomingNotificationsState === priorNotificationState &&
                incomingNotificationId === notificationId
                ? expectedNotificationState
                : badNotificationState
          );

        const result = reducer(priorExistingState, {
          type: "CLEAR_NOTIFICATION",
          payload: { notificationId: notificationId },
        });

        expect(result.notificationState).toBe(expectedNotificationState);
        expect(result.documentsState).toBe(expectedDocumentsState);
      });

      it("should delegate clearing a documents notifications to a function owned by the notifications code", () => {
        const documentId = "1";

        jest
          .spyOn(notificationsMappingFunctions, "clearDocumentNotifications")
          .mockImplementation(
            (incomingNotificationsState, incomingDocumentIdId) =>
              incomingNotificationsState === priorNotificationState &&
                incomingDocumentIdId === documentId
                ? expectedNotificationState
                : badNotificationState
          );

        const result = reducer(priorExistingState, {
          type: "CLEAR_DOCUMENT_NOTIFICATIONS",
          payload: { documentId },
        });

        expect(result.notificationState).toBe(expectedNotificationState);
        expect(result.documentsState).toBe(expectedDocumentsState);
      });
    });
  });

  it("can not handle an unknown action", () => {
    expect(() =>
      reducer({} as CombinedState, { type: "UNKNOWN" } as any)
    ).toThrow();
  });
});
