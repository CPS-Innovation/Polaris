/// <reference types="cypress" />
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"

const { REFRESH_LOCK_URN, REFRESH_LOCK_CASE_ID } = Cypress.env()

let routes: ApiRoutes

describe("Subsequent Refresh Locks", { tags: "@ci" }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("the pipeline runs as a singleton", () => {
    cy.clearCaseTracker(REFRESH_LOCK_URN, REFRESH_LOCK_CASE_ID)
      .api<{ trackerUrl: string }>(
        routes.TRACKER_START(REFRESH_LOCK_URN, REFRESH_LOCK_CASE_ID, "PHASE_1")
      )
      .then(({ status, body }) => {
        expect(status).to.equal(200)
        expect(body.trackerUrl).to.equal("/api/tracker")
      })
      .api({
        ...routes.TRACKER_START(
          REFRESH_LOCK_URN,
          REFRESH_LOCK_CASE_ID,
          "PHASE_1"
        ),
        failOnStatusCode: false,
      })
      .its("status")
      .then((status) => expect(status).to.equal(423))
  })
})
