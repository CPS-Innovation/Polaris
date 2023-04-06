/// <reference types="cypress" />
import "cypress-wait-until"
import { CaseDetails } from "../../../gateway/CaseDetails"
import { CaseSearchResult } from "../../../gateway/CaseSearchResult"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { CorrelationId, correlationIds } from "../../support/correlation-ids"
import { ApiTextSearchResult } from "../../../gateway/ApiTextSearchResult"
import { RedactionSaveRequest } from "../../../gateway/RedactionSaveRequest"

const WAIT_UNTIL_OPTIONS = { interval: 3 * 1000, timeout: 60 * 1000 }
const {
  API_ROOT_DOMAIN,
  TARGET_URN,
  TARGET_CASE_ID,
  REFRESH_TARGET_URN,
  REFRESH_TARGET_CASE_ID,
} = Cypress.env()

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
  documentId: string,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}/checkout`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
  method: "POST",
})

const SAVE_DOCUMENT = (
  urn: string,
  caseId: number,
  documentId: string,
  correlationId: CorrelationId = "BLANK"
) => ({
  url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/documents/${documentId}`,
  headers: { ...authHeaders, "correlation-id": correlationIds[correlationId] },
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

  it("the pipeline can clear then process a case to completion", () => {
    cy.api(TRACKER_CLEAR(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID))
      .wait(2000)
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(
              GET_TRACKER(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, "PHASE_1")
            )
            .its("body")
            .then(({ documents }) => documents.length === 0),
        WAIT_UNTIL_OPTIONS
      )
      .api(TRACKER_START(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, "PHASE_1"))
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(
              GET_TRACKER(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, "PHASE_1")
            )
            .its("body")
            .then(({ status }) => {
              if (status === "Failed") {
                throw new Error("Pipeline failed, ending test")
              }
              return status === "Completed"
            }),
        WAIT_UNTIL_OPTIONS
      )
      .api<PipelineResults>(
        GET_TRACKER(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, "PHASE_1")
      )
      .its("body")
      .then((results) => {
        cy.wrap(saveVariablesHelper(results)).as("phase1Vars")
        expect(
          results.documents.every((document) => document.status === "Indexed")
        ).to.equal(true)
      })

    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "one",
      docId: "numbersDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "two",
      docId: "numbersDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "three",
      docId: "numbersDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_NO_RESULTS",
      term: "four",
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "alice",
      docId: "peopleDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "bob",
      docId: "peopleDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "carol",
      docId: "peopleDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_NO_RESULTS",
      term: "dave",
    })

    cy.get<SavedVariables>("@phase1Vars").then((phase1Vars) => {
      cy.api(
        CHECKOUT_DOCUMENT(
          REFRESH_TARGET_URN,
          REFRESH_TARGET_CASE_ID,
          phase1Vars.numbersDocId,
          "PHASE_1"
        )
      )
        .api(
          SAVE_DOCUMENT(
            REFRESH_TARGET_URN,
            REFRESH_TARGET_CASE_ID,
            phase1Vars.numbersDocId,
            "PHASE_1"
          )
        )
        .api(
          CHECKOUT_DOCUMENT(
            REFRESH_TARGET_URN,
            REFRESH_TARGET_CASE_ID,
            phase1Vars.peopleDocId,
            "PHASE_1"
          )
        )
        .api(
          SAVE_DOCUMENT(
            REFRESH_TARGET_URN,
            REFRESH_TARGET_CASE_ID,
            phase1Vars.peopleDocId,
            "PHASE_1"
          )
        )
        .api(
          TRACKER_START(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, "PHASE_2")
        )
        .waitUntil(
          () =>
            cy
              .api<PipelineResults>(
                GET_TRACKER(
                  REFRESH_TARGET_URN,
                  REFRESH_TARGET_CASE_ID,
                  "PHASE_2"
                )
              )
              .its("body")
              .then(({ processingCompleted, status }) => {
                if (status === "Failed") {
                  throw new Error("Pipeline failed, ending test")
                }
                return (
                  processingCompleted &&
                  processingCompleted !== phase1Vars.processingCompleted
                )
              }),
          WAIT_UNTIL_OPTIONS
        )
    })

    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "one",
      docId: "numbersDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "two",
      docId: "numbersDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_NO_RESULTS",
      term: "three",
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "four",
      docId: "numbersDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "alice",
      docId: "peopleDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "bob",
      docId: "peopleDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_NO_RESULTS",
      term: "carol",
    })
    searchAssertion({
      type: "EXPECT_RESULTS",
      term: "dave",
      docId: "peopleDocId",
      quatity: 1,
    })
  })
})

type SavedVariables = ReturnType<typeof saveVariablesHelper>
const saveVariablesHelper = ({
  documents,
  processingCompleted,
}: PipelineResults) => {
  return {
    processingCompleted,
    peopleDocId: documents.find((doc) =>
      doc.cmsOriginalFileName.includes("people")
    ).polarisDocumentId,
    numbersDocId: documents.find((doc) =>
      doc.cmsOriginalFileName.includes("numbers")
    ).polarisDocumentId,
  }
}

type SearchAssertionArg =
  | {
      type: "EXPECT_RESULTS"
      term: string
      docId: keyof Extract<SavedVariables, "peopleDocId" | "numbersDocId">
      quatity: number
    }
  | {
      type: "EXPECT_NO_RESULTS"
      term: string
    }

const searchAssertion = (arg: SearchAssertionArg) => {
  cy.api<ApiTextSearchResult[]>(
    GET_SEARCH(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, arg.term)
  )
    .its("body")
    .then((results) => {
      cy.get<SavedVariables>("@phase1Vars").then((phase1Vars) => {
        if (arg.type === "EXPECT_NO_RESULTS") {
          expect(results.length).to.equal(0)
          return
        } else {
          expect(results.length).to.equal(arg.quatity)
          expect(results[0].polarisDocumentId).to.equal(phase1Vars[arg.docId])
        }
      })
    })
}

export {}
