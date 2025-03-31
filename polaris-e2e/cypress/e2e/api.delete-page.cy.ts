/// <reference types="cypress" />
import "cypress-wait-until"
import { ApiRoutes, makeApiRoutes } from "../support/helpers/make-routes"
import { PipelineResults } from "../../gateway/PipelineResults"
import { isTrackerReady } from "../support/helpers/tracker-helpers"
import { RAPID_RETRY_WAIT_UNTIL_OPTIONS } from "../support/options"

const { DELETE_PAGE_TARGET_URN, DELETE_PAGE_CASE_ID, DELETE_PAGE_DOCUMENT_ID, DELETE_PAGE_VERSION_ID } = Cypress.env()

let routes: ApiRoutes

describe("Delete Page", { tags: ["@ci", "@ci-chunk-4"] }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("can delete a page for a Document", () => {
    cy.clearCaseTracker(
      DELETE_PAGE_TARGET_URN,
      DELETE_PAGE_CASE_ID
    )
      .api(
        routes.TRACKER_START(
          DELETE_PAGE_TARGET_URN,
          DELETE_PAGE_CASE_ID
        )
      )
      .waitUntil(
        () =>
          cy
            .api<PipelineResults>({
              ...routes.GET_TRACKER(
                DELETE_PAGE_TARGET_URN,
                DELETE_PAGE_CASE_ID
              ),
              failOnStatusCode: false,
            })
            .then(isTrackerReady),
        // RAPID... to try to minimise the gap between when the pipeline is complete and
        //  when we know about it and start searching
        RAPID_RETRY_WAIT_UNTIL_OPTIONS
      )

    cy.api(routes.DELETE_PAGE(DELETE_PAGE_TARGET_URN, DELETE_PAGE_CASE_ID, DELETE_PAGE_DOCUMENT_ID, DELETE_PAGE_VERSION_ID, "PHASE_1"))
      .then(({ status }) => {
        expect(status).to.equal(200);
      })
  })
})
