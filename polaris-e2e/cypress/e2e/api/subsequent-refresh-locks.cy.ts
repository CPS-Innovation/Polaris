/// <reference types="cypress" />
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"

const { REFRESH_LOCK_URN, REFRESH_LOCK_CASE_ID } = Cypress.env();

let routes: ApiRoutes

describe("Subsequent Refresh Locks", { tags: '@ci' }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("the pipeline runs as a singleton", () => {
    cy.clearCaseTracker(REFRESH_LOCK_URN, REFRESH_LOCK_CASE_ID)
      .api(
        routes.TRACKER_START(REFRESH_LOCK_URN, REFRESH_LOCK_CASE_ID, "PHASE_1")
      )
      .its("status")
      .then((status) => status === 202)
      .api({
        ...routes.TRACKER_START(
          REFRESH_LOCK_URN,
          REFRESH_LOCK_CASE_ID,
          "PHASE_1"
        ),
        failOnStatusCode: false,
      })
      .its("status")
      .then((status) => status === 403);
  })
})
