/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"

const { TARGET_URN, TARGET_CASE_ID } = Cypress.env()

let routes: ApiRoutes

describe("Simple Tracker", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("run a trackerthrough to all documents being indexed", () => {
    cy.api(routes.TRACKER_CLEAR(TARGET_URN, TARGET_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>({
              ...routes.GET_TRACKER(TARGET_URN, TARGET_CASE_ID),
              failOnStatusCode: false,
            })
            .its("status")
            .then((status) => status === 404),
        WAIT_UNTIL_OPTIONS
      )
      .api(routes.TRACKER_START(TARGET_URN, TARGET_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api({
              ...routes.GET_TRACKER(TARGET_URN, TARGET_CASE_ID),
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
