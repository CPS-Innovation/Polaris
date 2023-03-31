/// <reference types="cypress" />
import "cypress-wait-until"
import { CaseDetails } from "../../../gateway/CaseDetails"
import { CaseSearchResult } from "../../../gateway/CaseSearchResult"
import { PipelineResults } from "../../../gateway/PipelineResults"

const WAIT_UNTIL_OPTIONS = { interval: 5 * 1000, timeout: 60 * 1000 }
const { TARGET_URN, API_ROOT_DOMAIN, TARGET_CASE_ID } = Cypress.env()

// allow headers to be passed from beforeEach to tests
let headers

const LIST_CASES = () => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases`,
  headers,
})

const GET_CASE = (caseId: number) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${caseId}`,
  headers,
})

const TRACKER_CLEAR = () => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${TARGET_CASE_ID}`,
  headers,
  method: "DELETE",
})

const TRACKER_START = () => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${TARGET_CASE_ID}`,
  headers,
  method: "POST",
})

const GET_TRACKER = () => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${TARGET_CASE_ID}/tracker`,
  headers,
})

describe("Api tests", () => {
  beforeEach(() => {
    cy.getApiHeaders().then((apiHeaders) => {
      headers = apiHeaders
    })
  })

  it("can list cases for a URN and then retrieve the first case of the URN", () => {
    cy.api<CaseSearchResult[]>(LIST_CASES())
      .then(({ body }) => cy.api<CaseDetails>(GET_CASE(body[0].id)))
      .then(({ body }) => {
        expect(body.uniqueReferenceNumber).to.equal(TARGET_URN)
      })
  })

  it("can process a case", () => {
    cy.api(TRACKER_CLEAR())
      .waitUntil(
        () =>
          cy
            .api(GET_TRACKER())
            .its("body.documents")
            .then((documents) => documents.length === 0),
        WAIT_UNTIL_OPTIONS
      )
      .api(TRACKER_START())
      .waitUntil(
        () =>
          cy
            .api(GET_TRACKER())
            .its("body.status")
            .then((status) => status === "Completed"),
        WAIT_UNTIL_OPTIONS
      )
      .api<PipelineResults>(GET_TRACKER())
      .then(({ body }) => {
        expect(
          body.documents.every((document) => document.status === "Indexed")
        ).to.equal(true)
      })
  })
})

export {}
