/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiTextSearchResult } from "../../../gateway/ApiTextSearchResult"
import { makeRoutes } from "./helpers/make-routes"

const WAIT_UNTIL_OPTIONS = { interval: 3 * 1000, timeout: 60 * 1000 }

const { REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID } = Cypress.env()

let routes

describe("Refresh", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeRoutes(headers)
    })
  })

  it("the pipeline can clear then process a case to completion", () => {
    cy.log("================== REFRESH TEST STARTED ====================")
    cy.log("------------------------ GET CASE --------------------------")
    cy.api(routes.TRACKER_CLEAR(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID))
      .wait(2000)
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>({
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
            .api<PipelineResults>({
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
      .then((results) => {
        cy.wrap(saveVariablesHelper(results)).as("phase1Vars")
        const documentTypes = results.documents.map(
          (doc) => doc.cmsDocType.documentType
        )
        const cmsDocExists =
          documentTypes.findIndex((val) => val == "MG 3") > -1
        const pcdDocExists = documentTypes.findIndex((val) => val == "PCD") > -1
        const allDocsIndexed = results.documents.every(
          (document) => document.status === "Indexed"
        )
        expect(cmsDocExists && pcdDocExists && allDocsIndexed).to.equal(true)
        expect(cmsDocExists && pcdDocExists && allDocsIndexed).to.equal(true)
      })
      .log("CMS and PCD Documents processed OK")

    cy.log(
      "-------------------- SEARCH UNREDACTED CASE DOCUMENTS --------------------"
    )
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

    cy.log("-------------------- 1ST DOCUMENT REDACTION  --------------------")
    cy.get<SavedVariables>("@phase1Vars").then((phase1Vars) => {
      cy.api(
        routes.CHECKOUT_DOCUMENT(
          REFRESH_TARGET_URN,
          REFRESH_TARGET_CASE_ID,
          phase1Vars.numbersDocId,
          "PHASE_2"
        )
      )
        .api(
          routes.SAVE_DOCUMENT(
            REFRESH_TARGET_URN,
            REFRESH_TARGET_CASE_ID,
            phase1Vars.numbersDocId,
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
              .then((results) => {
                if (results.status === "Failed") {
                  throw new Error("Pipeline failed, ending test")
                }
                return (
                  results.processingCompleted &&
                  results.processingCompleted !== phase1Vars.processingCompleted
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
        .then((results) => {
          cy.wrap(saveVariablesHelper(results)).as("phase2Vars")
          const documentTypes = results.documents.map(
            (doc) => doc.cmsDocType.documentType
          )
          const cmsDocExists =
            documentTypes.findIndex((val) => val == "MG 3") > -1
          const pcdDocExists =
            documentTypes.findIndex((val) => val == "PCD") > -1
          const allDocsIndexed = results.documents.every(
            (document) => document.status === "Indexed"
          )
          expect(cmsDocExists && pcdDocExists && allDocsIndexed).to.equal(true)
        })
    })

    cy.log(
      "-------------------- VALIDATE 1ST DOCUMENT REDACTION --------------------"
    )
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
      type: "EXPECT_RESULTS",
      term: "carol",
      docId: "peopleDocId",
      quatity: 1,
    })
    searchAssertion({
      type: "EXPECT_NO_RESULTS",
      term: "dave",
    })

    cy.log("-------------------- 2ND DOCUMENT REDACTION  --------------------")
    cy.get<SavedVariables>("@phase2Vars").then((phase2Vars) => {
      cy.api(
        routes.CHECKOUT_DOCUMENT(
          REFRESH_TARGET_URN,
          REFRESH_TARGET_CASE_ID,
          phase2Vars.peopleDocId,
          "PHASE_3"
        )
      )
        .api(
          routes.SAVE_DOCUMENT(
            REFRESH_TARGET_URN,
            REFRESH_TARGET_CASE_ID,
            phase2Vars.peopleDocId,
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
              .then((results) => {
                if (results.status === "Failed") {
                  throw new Error("Pipeline failed, ending test")
                }
                return (
                  results.processingCompleted &&
                  results.processingCompleted !== phase2Vars.processingCompleted
                )
              }),
          WAIT_UNTIL_OPTIONS
        )
    })

    cy.log(
      "-------------------- VALIDATE 2ND DOCUMENT REDACTION  --------------------"
    )
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

    cy.log("================ REFRESH TEST COMPLETED ==================")
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
    routes.GET_SEARCH(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, arg.term)
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
