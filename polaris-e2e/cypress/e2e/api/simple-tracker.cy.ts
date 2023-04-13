/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { makeRoutes } from "./helpers/make-routes"

const WAIT_UNTIL_OPTIONS = { interval: 3 * 1000, timeout: 60 * 1000 }

const { TARGET_URN, TARGET_CASE_ID } = Cypress.env()

let routes

describe("Simple Tracker", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeRoutes(headers)
    })
  })

  it("run a trackerthrough to all documents being indexed", () => {
    cy.api(routes.TRACKER_CLEAR(TARGET_URN, TARGET_CASE_ID))
      .wait(2000)
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(
              routes.GET_TRACKER(TARGET_URN, TARGET_CASE_ID)
            )
            .its("body")
            .then(({ documents }) => documents.length === 0),
        WAIT_UNTIL_OPTIONS
      )
      .api(routes.TRACKER_START(TARGET_URN, TARGET_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>(
              routes.GET_TRACKER(TARGET_URN, TARGET_CASE_ID)
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
  })
})
