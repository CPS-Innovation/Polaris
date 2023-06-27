/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiTextSearchResult } from "../../../gateway/ApiTextSearchResult"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"

const { REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, PRE_SEARCH_DELAY_MS } =
  Cypress.env()

let routes: ApiRoutes

describe("Refresh", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("the pipeline can clear then process a case to completion", () => {
    cy.clearCaseTracker(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID)
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
            .api({
              ...routes.GET_TRACKER(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID),
              failOnStatusCode: false,
            })
            .its("status")
            .then((status) => status !== 404),
        WAIT_UNTIL_OPTIONS
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

        expect(
          documents.every(({ status }) => status === "Indexed"),
          "All documents are indexed in PHASE_1"
        ).to.be.true
        expect(
          documents.some(
            ({ cmsDocType }) => cmsDocType.documentType === "MG 5"
          ),
          "MG 5 is presen in PHASE_1"
        ).to.be.true
        expect(
          documents.some(({ cmsDocType }) => cmsDocType.documentType === "PCD"),
          "PCD is present in PHASE_1"
        ).to.be.true
        expect(
          documents.some(({ cmsDocType }) => cmsDocType.documentType === "DAC"),
          "DAC is present in PHASE_1"
        ).to.be.true
      })

    cy.wait(PRE_SEARCH_DELAY_MS)
    // todo: #21848 - early warning that the Aspose licence has expired (in which case a watermark is added to the document
    //  that the OCR process will read and add to the index - the watermark includes the word "Aspose").
    //  todo: move this test to a better and dedicated test suite (e.g. pdf service integration test)
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "aspose",
      phase: "PHASE_1",
    })

    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "one",
      docId: "numbersDocId",
      phase: "PHASE_1",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "two",
      docId: "numbersDocId",
      phase: "PHASE_1",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "three",
      docId: "numbersDocId",
      phase: "PHASE_1",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "four",
      phase: "PHASE_1",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "alice",
      docId: "peopleDocId",
      phase: "PHASE_1",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "bob",
      docId: "peopleDocId",
      phase: "PHASE_1",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "carol",
      docId: "peopleDocId",
      phase: "PHASE_1",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "dave",
      phase: "PHASE_1",
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
            expect(
              documents.every(({ status }) => status === "Indexed"),
              "All documents are indexed in PHASE_2"
            ).to.be.true
            expect(
              documents.some(
                ({ cmsDocType }) => cmsDocType.documentType === "MG 5"
              ),
              "MG 5 is present in PHASE_2"
            ).to.be.true
            expect(
              documents.some(
                ({ cmsDocType }) => cmsDocType.documentType === "PCD"
              ),
              "PCD is present in PHASE_2"
            ).to.be.true
            expect(
              documents.some(
                ({ cmsDocType }) => cmsDocType.documentType === "DAC"
              ),
              "DAC is present in PHASE_2"
            ).to.be.true
          })
      }
    )

    cy.wait(PRE_SEARCH_DELAY_MS)

    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "one",
      docId: "numbersDocId",
      phase: "PHASE_2",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "two",
      docId: "numbersDocId",
      phase: "PHASE_2",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "three",
      phase: "PHASE_2",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "four",
      docId: "numbersDocId",
      phase: "PHASE_2",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "alice",
      docId: "peopleDocId",
      phase: "PHASE_2",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "bob",
      docId: "peopleDocId",
      phase: "PHASE_2",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "carol",
      docId: "peopleDocId",
      phase: "PHASE_2",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "dave",
      phase: "PHASE_2",
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

    cy.wait(PRE_SEARCH_DELAY_MS)

    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "one",
      docId: "numbersDocId",
      phase: "PHASE_3",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "two",
      docId: "numbersDocId",
      phase: "PHASE_3",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "three",
      phase: "PHASE_3",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "four",
      docId: "numbersDocId",
      phase: "PHASE_3",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "alice",
      docId: "peopleDocId",
      phase: "PHASE_3",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "bob",
      docId: "peopleDocId",
      phase: "PHASE_3",
    })
    assertSearchExpectation({
      expectation: "TERM_NOT_PRESENT",
      term: "carol",
      phase: "PHASE_3",
    })
    assertSearchExpectation({
      expectation: "TERM_PRESENT_IN_DOC_ID",
      term: "dave",
      docId: "peopleDocId",
      phase: "PHASE_3",
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
      phase: "PHASE_1" | "PHASE_2" | "PHASE_3"
    }
  | {
      expectation: "TERM_NOT_PRESENT"
      term: string
      phase: "PHASE_1" | "PHASE_2" | "PHASE_3"
    }

const assertSearchExpectation = (arg: SearchAssertionArg) => {
  cy.api<ApiTextSearchResult[]>(
    routes.GET_SEARCH(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, arg.term)
  )
    .its("body")
    .then((results) => {
      cy.get<SavedVariables>("@phase1Vars").then((phase1Vars) => {
        if (arg.expectation === "TERM_NOT_PRESENT") {
          expect(results.length).to.equal(
            0,
            `Results for "${arg.term}" in phase ${arg.phase}`
          )
        } else {
          expect(results.length).to.equal(
            1,
            `Results for "${arg.term}" in phase ${arg.phase}`
          )
          expect(results[0].polarisDocumentId).to.equal(
            phase1Vars[arg.docId],
            `Doc id check in phase ${arg.phase}`
          )
        }
      })
    })
}
