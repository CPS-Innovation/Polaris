/// <reference types="cypress" />
import "cypress-wait-until"
import { CaseDetails } from "../../../gateway/CaseDetails"
import { CaseSearchResult } from "../../../gateway/CaseSearchResult"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { CorrelationId, correlationIds } from "../../support/correlation-ids"

const WAIT_UNTIL_OPTIONS = { interval: 5 * 1000, timeout: 60 * 1000 }
const { TARGET_URN, API_ROOT_DOMAIN, TARGET_CASE_ID } = Cypress.env()

// allow headers to be passed from beforeEach to tests
let authHeaders

const LIST_CASES = (correlationId: CorrelationId = "BLANK") => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
})

const GET_CASE = (caseId: number, correlationId: CorrelationId = "BLANK") => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${caseId}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
})

const TRACKER_CLEAR = (correlationId: CorrelationId = "BLANK") => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${TARGET_CASE_ID}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
  method: "DELETE",
})

const TRACKER_START = (correlationId: CorrelationId = "BLANK") => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${TARGET_CASE_ID}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
  method: "POST",
})

const GET_TRACKER = (correlationId: CorrelationId = "BLANK") => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${TARGET_CASE_ID}/tracker`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
})

describe("API tests", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((authHeaders) => {
      authHeaders = authHeaders
    })
  })

  it("can list cases for a URN and then retrieve the first case of the URN", () => {
    cy.api<CaseSearchResult[]>(LIST_CASES())
      .then(({ body }) => cy.api<CaseDetails>(GET_CASE(body[0].id)))
      .then(({ body }) => {
        expect(body.uniqueReferenceNumber).to.equal(TARGET_URN)
      })
  })

  it("the pipeline can clear then process a case to completion", () => {
    cy.api(TRACKER_CLEAR())
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(GET_TRACKER())
            .its("body")
            .then(({ documents }) => documents.length === 0),
        WAIT_UNTIL_OPTIONS
      )
      .api(TRACKER_START())
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(GET_TRACKER())
            .its("body")
            .then(({ status }) => status === "Completed"),
        WAIT_UNTIL_OPTIONS
      )
      .api<PipelineResults>(GET_TRACKER())
      .its("body")
      .then(({ documents }) => {
        expect(
          documents.every((document) => document.status === "Indexed")
        ).to.equal(true)
      })
  })
})

export {}
