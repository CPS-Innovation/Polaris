/// <reference types="cypress" />
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"

const { REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID } = Cypress.env()

let routes: ApiRoutes

describe("Subsequent Refresh Locks", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("the pipeline runs as a singleton", () => {
    cy.clearCaseTracker(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID)
      .api(
        routes.TRACKER_START(
          REFRESH_TARGET_URN,
          REFRESH_TARGET_CASE_ID,
          "PHASE_1"
        )
      )
      .its("status")
      .then((status) => status === 202)
      .api({
        ...routes.TRACKER_START(
          REFRESH_TARGET_URN,
          REFRESH_TARGET_CASE_ID,
          "PHASE_1"
        ),
        failOnStatusCode: false,
      })
      .its("status")
      .then((status) => status === 403)
  })
})
