import { RedactionSaveRequest } from "../../../../gateway/RedactionSaveRequest"
import { CorrelationId, correlationIds } from "../../../support/correlation-ids"

const { API_ROOT_DOMAIN } = Cypress.env()

export type ApiRoutes = ReturnType<typeof makeApiRoutes>

export const makeApiRoutes = (authHeaders: any) => {
  const LIST_CASES = (urn: string, correlationId: CorrelationId = "BLANK") => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
  })

  const GET_CASE = (
    urn: string,
    caseId: number,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
  })

  // const TRACKER_CLEAR = (
  //   urn: string,
  //   caseId: number,
  //   correlationId: CorrelationId = "BLANK"
  // ) => ({
  //   url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
  //   headers: {
  //     ...authHeaders,
  //     "correlation-id": correlationIds[correlationId],
  //   },
  //   method: "DELETE",
  // })

  const TRACKER_START = (
    urn: string,
    caseId: number,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
    method: "POST",
  })

  const GET_TRACKER = (
    urn: string,
    caseId: number,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/tracker`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
  })

  const GET_SEARCH = (
    urn: string,
    caseId: number,
    query: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/search?query=${query}`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
  })

  const CHECKOUT_DOCUMENT = (
    urn: string,
    caseId: number,
    documentId: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}/checkout`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
    method: "POST",
  })

  const SAVE_DOCUMENT = (
    urn: string,
    caseId: number,
    documentId: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
    method: "PUT",
    body: {
      documentId,
      redactions: [
        {
          pageIndex: 1,
          height: 1,
          width: 1,
          redactionCoordinates: [{ x1: 0, y1: 0, x2: 1, y2: 1 }],
        },
      ],
    } as RedactionSaveRequest,
  })

  const GET_DOCUMENT_SAS_LINK = (
    urn: string,
    caseId: number,
    documentId: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}/sas-url`,
    headers: {
      ...authHeaders,
      "correlation-id": correlationIds[correlationId],
    },
    method: "GET",
  })

  return {
    LIST_CASES,
    GET_CASE,
    //TRACKER_CLEAR,
    TRACKER_START,
    GET_TRACKER,
    GET_SEARCH,
    CHECKOUT_DOCUMENT,
    SAVE_DOCUMENT,
    GET_DOCUMENT_SAS_LINK,
  }
}
