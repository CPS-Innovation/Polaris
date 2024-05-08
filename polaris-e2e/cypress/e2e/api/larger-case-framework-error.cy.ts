/// <reference types="cypress" />
import { PipelineResults } from "../../../gateway/PipelineResults"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { isTrackerReady } from "./helpers/tracker-helpers"

const { LARGE_CASE_URN, LARGE_CASE_ID, LARGE_CASE_DOCUMENT_ID } = Cypress.env()

let routes: ApiRoutes

describe("Larger cases", { tags: "@ci" }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  // see incident covered by feature #27053 for the motivation for this test
  it("the durable framework copes with larger cases", () => {
    cy.clearCaseTracker(LARGE_CASE_URN, LARGE_CASE_ID)
      .api<{ trackerUrl: string }>(
        routes.TRACKER_START(LARGE_CASE_URN, LARGE_CASE_ID)
      )
      .then(({ status, body }) => {
        expect(status).to.equal(200)
        expect(body.trackerUrl).to.equal(
          `/api/urns/${LARGE_CASE_URN}/cases/${LARGE_CASE_ID}/tracker`
        )
      })
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>({
              ...routes.GET_TRACKER(LARGE_CASE_URN, LARGE_CASE_ID),
              failOnStatusCode: false,
            })
            .then(isTrackerReady),
        WAIT_UNTIL_OPTIONS
      )
      .api(
        routes.GET_DOCUMENT(
          LARGE_CASE_URN,
          LARGE_CASE_ID,
          LARGE_CASE_DOCUMENT_ID
        )
      )
      .then((response) => {
        expect(response.status).to.equal(200)
        expect(response.headers["content-type"]).to.equal("application/pdf")
        expect(String(response.body).length).to.be.greaterThan(1000)
      })
  })
})
