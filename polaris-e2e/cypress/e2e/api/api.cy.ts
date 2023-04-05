/// <reference types="cypress" />
import "cypress-wait-until"
import { CaseDetails } from "../../../gateway/CaseDetails"
import { CaseSearchResult } from "../../../gateway/CaseSearchResult"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { CorrelationId, correlationIds } from "../../support/correlation-ids"

const WAIT_UNTIL_OPTIONS = { interval: 3 * 1000, timeout: 60 * 1000 }
const { TARGET_URN, API_ROOT_DOMAIN, TARGET_CASE_ID } = Cypress.env()

// allow headers to be passed from beforeEach to tests
let authHeaders

const LIST_CASES = (urn: string, correlationId: CorrelationId = "BLANK") => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
})

const GET_CASE = (
  urn: string,
  caseId: number,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
})

const TRACKER_CLEAR = (
  urn: string,
  caseId: number,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
  method: "DELETE",
})

const TRACKER_START = (
  urn: string,
  caseId: number,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
  method: "POST",
})

const GET_TRACKER = (
  urn: string,
  caseId: number,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/tracker`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
})

const GET_SEARCH = (
  urn: string,
  caseId: number,
  query: string,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/search?query=${query}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
})

const CHECKOUT_DOCUMENT = (
  urn: string,
  caseId: number,
  documentId: number,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}/checkout`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
  method: "POST",
})

describe("API tests", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      authHeaders = headers
    })
  })

  it("can list cases for a URN and then retrieve the first case of the URN", () => {
    cy.api<CaseSearchResult[]>(LIST_CASES(TARGET_URN, "PHASE_1"))
      .then(({ body }) =>
        cy.api<CaseDetails>(GET_CASE(TARGET_URN, TARGET_CASE_ID, "PHASE_1"))
      )
      .then(({ body }) => {
        expect(body.uniqueReferenceNumber).to.equal(TARGET_URN)
      })
  })

  it.only("the pipeline can clear then process a case to completion", () => {
    cy.api(TRACKER_CLEAR(TARGET_URN, TARGET_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(
              GET_TRACKER(TARGET_URN, TARGET_CASE_ID, "PHASE_1")
            )
            .its("body")
            .then(({ documents }) => documents.length === 0),
        WAIT_UNTIL_OPTIONS
      )
      .api(TRACKER_START(TARGET_URN, TARGET_CASE_ID, "PHASE_1"))
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(
              GET_TRACKER(TARGET_URN, TARGET_CASE_ID, "PHASE_1")
            )
            .its("body")
            .then(({ status }) => status === "Completed"),
        WAIT_UNTIL_OPTIONS
      )
      .api<PipelineResults>(GET_TRACKER(TARGET_URN, TARGET_CASE_ID, "PHASE_1"))
      .its("body")
      .then(({ documents }) => {
        expect(
          documents.every((document) => document.status === "Indexed")
        ).to.equal(true)
      })
      .api<CaseSearchResult[]>(GET_SEARCH(TARGET_URN, TARGET_CASE_ID, "one"))
      .its("body")
      .then((results) => {
        cy.log(String(results.length))
      })
      .api<CaseSearchResult[]>(GET_SEARCH(TARGET_URN, TARGET_CASE_ID, "two"))
      .its("body")
      .then((results) => {
        cy.log(String(results.length))
      })
      .api<CaseSearchResult[]>(GET_SEARCH(TARGET_URN, TARGET_CASE_ID, "three"))
      .its("body")
      .then((results) => {
        cy.log(String(results.length))
      })
  })
})

export {}
