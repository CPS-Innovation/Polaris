/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"

const { WITNESS_TARGET_URN, WITNESS_TARGET_CASE_ID } = Cypress.env()

let routes: ApiRoutes

describe("A case with witnesses", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("can retrieve witnesses and document details with witness ids", () => {
    cy.clearCaseTracker(WITNESS_TARGET_URN, WITNESS_TARGET_CASE_ID)
      .api(routes.TRACKER_START(WITNESS_TARGET_URN, WITNESS_TARGET_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api({
              ...routes.GET_TRACKER(WITNESS_TARGET_URN, WITNESS_TARGET_CASE_ID),
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
              routes.GET_TRACKER(WITNESS_TARGET_URN, WITNESS_TARGET_CASE_ID)
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
