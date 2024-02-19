import { RedactionSaveRequest } from "../../../../gateway/RedactionSaveRequest"
import { CorrelationId, correlationIds } from "../../../support/correlation-ids"

const { API_ROOT_DOMAIN } = Cypress.env()

export type ApiRoutes = ReturnType<typeof makeApiRoutes>

const makeHeaders = (headers: any, correlationId: CorrelationId) => ({
  ...headers,
  "correlation-id": correlationIds[correlationId],
})

export const makeApiRoutes = (headers: any) => {
  const LIST_CASES = (urn: string, correlationId: CorrelationId = "BLANK") => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases`,
    headers: makeHeaders(headers, correlationId),
  })

  const GET_CASE = (
    urn: string,
    caseId: number,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
    headers: makeHeaders(headers, correlationId),
  })

  const TRACKER_START = (
    urn: string,
    caseId: number,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
    headers: makeHeaders(headers, correlationId),
    method: "POST",
  })

  const GET_TRACKER = (
    urn: string,
    caseId: number,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/tracker`,
    headers: makeHeaders(headers, correlationId),
  })

  const GET_SEARCH = (
    urn: string,
    caseId: number,
    query: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/search?query=${query}`,
    headers: makeHeaders(headers, correlationId),
  })

  const GET_DOCUMENT = (
    urn: string,
    caseId: number,
    documentId: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}`,
    headers: makeHeaders(headers, correlationId),
    method: "GET",
  })

  const CHECKOUT_DOCUMENT = (
    urn: string,
    caseId: number,
    documentId: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}/checkout`,
    headers: makeHeaders(headers, correlationId),
    method: "POST",
  })

  const CANCEL_CHECKOUT_DOCUMENT = (
    urn: string,
    caseId: number,
    documentId: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}/checkout`,
    headers: makeHeaders(headers, correlationId),
    method: "DELETE",
  })

  const SAVE_DOCUMENT = (
    urn: string,
    caseId: number,
    documentId: string,
    correlationId: CorrelationId = "BLANK"
  ) => ({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}`,
    headers: makeHeaders(headers, correlationId),
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

  return {
    LIST_CASES,
    GET_CASE,
    //TRACKER_CLEAR,
    TRACKER_START,
    GET_TRACKER,
    GET_SEARCH,
    GET_DOCUMENT,
    CHECKOUT_DOCUMENT,
    CANCEL_CHECKOUT_DOCUMENT,
    SAVE_DOCUMENT,
  }
}
