/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiTextSearchResult } from "../../../gateway/ApiTextSearchResult"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"

const { REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID } = Cypress.env()

let routes: ApiRoutes

describe("Refresh", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("the pipeline can clear then process a case to completion", () => {
    cy.api(routes.TRACKER_CLEAR(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api({
              ...routes.GET_TRACKER(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID),
              failOnStatusCode: false,
            })
            .its("status")
            .then((status) => status === 404),
        WAIT_UNTIL_OPTIONS
      )
      .api(
        routes.TRACKER_START(
          REFRESH_TARGET_URN,
          REFRESH_TARGET_CASE_ID,
          "PHASE_1"
        )
      )
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(
              routes.GET_TRACKER(
                REFRESH_TARGET_URN,
                REFRESH_TARGET_CASE_ID,
                "PHASE_1"
              )
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
        routes.GET_TRACKER(
          REFRESH_TARGET_URN,
          REFRESH_TARGET_CASE_ID,
          "PHASE_1"
        )
      )
      .its("body")
      .then(({ documents, processingCompleted }) => {
        cy.wrap(saveVariablesHelper({ documents, processingCompleted })).as(
          "phase1Vars"
        )

        expect(documents.every(({ status }) => status === "Indexed")).to.be.true
        expect(
          documents.some(({ cmsDocType }) => cmsDocType.documentType === "MG 3")
        ).to.be.true
        expect(
          documents.some(({ cmsDocType }) => cmsDocType.documentType === "PCD")
        ).to.be.true
      })

    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "one",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "two",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "three",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "four",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "alice",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "bob",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "carol",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "dave",
    })

    cy.get<SavedVariables>("@phase1Vars").then(
      ({ numbersDocId, previousProcessingCompleted }) => {
        cy.api(
          routes.CHECKOUT_DOCUMENT(
            REFRESH_TARGET_URN,
            REFRESH_TARGET_CASE_ID,
            numbersDocId,
            "PHASE_2"
          )
        )
          .api(
            routes.SAVE_DOCUMENT(
              REFRESH_TARGET_URN,
              REFRESH_TARGET_CASE_ID,
              numbersDocId,
              "PHASE_2"
            )
          )
          .api(
            routes.TRACKER_START(
              REFRESH_TARGET_URN,
              REFRESH_TARGET_CASE_ID,
              "PHASE_2"
            )
          )
          .waitUntil(
            () =>
              cy
                .api<PipelineResults>(
                  routes.GET_TRACKER(
                    REFRESH_TARGET_URN,
                    REFRESH_TARGET_CASE_ID,
                    "PHASE_2"
                  )
                )
                .its("body")
                .then(({ status, processingCompleted }) => {
                  if (status === "Failed") {
                    throw new Error("Pipeline failed, ending test")
                  }
                  return (
                    processingCompleted &&
                    processingCompleted !== previousProcessingCompleted
                  )
                }),
            WAIT_UNTIL_OPTIONS
          )
          .api<PipelineResults>(
            routes.GET_TRACKER(
              REFRESH_TARGET_URN,
              REFRESH_TARGET_CASE_ID,
              "PHASE_2"
            )
          )
          .its("body")
          .then(({ documents, processingCompleted }) => {
            cy.wrap(saveVariablesHelper({ documents, processingCompleted })).as(
              "phase2Vars"
            )
            expect(documents.every(({ status }) => status === "Indexed")).to.be
              .true
            expect(
              documents.some(
                ({ cmsDocType }) => cmsDocType.documentType === "MG 3"
              )
            ).to.be.true
            expect(
              documents.some(
                ({ cmsDocType }) => cmsDocType.documentType === "PCD"
              )
            ).to.be.true
          })
      }
    )

    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "one",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "two",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "three",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "four",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "alice",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "bob",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "carol",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "dave",
    })

    cy.get<SavedVariables>("@phase2Vars").then(
      ({ peopleDocId, previousProcessingCompleted }) => {
        cy.api(
          routes.CHECKOUT_DOCUMENT(
            REFRESH_TARGET_URN,
            REFRESH_TARGET_CASE_ID,
            peopleDocId,
            "PHASE_3"
          )
        )
          .api(
            routes.SAVE_DOCUMENT(
              REFRESH_TARGET_URN,
              REFRESH_TARGET_CASE_ID,
              peopleDocId,
              "PHASE_3"
            )
          )
          .api(
            routes.TRACKER_START(
              REFRESH_TARGET_URN,
              REFRESH_TARGET_CASE_ID,
              "PHASE_3"
            )
          )
          .waitUntil(
            () =>
              cy
                .api<PipelineResults>(
                  routes.GET_TRACKER(
                    REFRESH_TARGET_URN,
                    REFRESH_TARGET_CASE_ID,
                    "PHASE_3"
                  )
                )
                .its("body")
                .then(({ status, processingCompleted }) => {
                  if (status === "Failed") {
                    throw new Error("Pipeline failed, ending test")
                  }
                  return (
                    processingCompleted &&
                    processingCompleted !== previousProcessingCompleted
                  )
                }),
            WAIT_UNTIL_OPTIONS
          )
      }
    )

    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "one",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "two",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "three",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "four",
      docId: "numbersDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "alice",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "bob",
      docId: "peopleDocId",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "carol",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "dave",
      docId: "peopleDocId",
    })
  })
})

type SavedVariables = ReturnType<typeof saveVariablesHelper>
const saveVariablesHelper = ({
  documents,
  processingCompleted,
}: Pick<PipelineResults, "documents" | "processingCompleted">) => {
  return {
    previousProcessingCompleted: processingCompleted,
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
      expectation: "TERM_PRESENT_IN_DOC_ID"
      term: string
      docId: keyof Extract<SavedVariables, "peopleDocId" | "numbersDocId">
    }
  | {
      expectation: "TERM_NOT_PRESENT"
      term: string
    }

const assertSearchExpectation = (arg: SearchAssertionArg) => {
  cy.api<ApiTextSearchResult[]>(
    routes.GET_SEARCH(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, arg.term)
  )
    .its("body")
    .then((results) => {
      cy.get<SavedVariables>("@phase1Vars").then((phase1Vars) => {
        if (arg.expectation === "TERM_NOT_PRESENT") {
          expect(results.length).to.equal(0)
        } else {
          expect(results.length).to.equal(1)
          expect(results[0].polarisDocumentId).to.equal(phase1Vars[arg.docId])
        }
      })
    })
}
