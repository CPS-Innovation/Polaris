/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"

const {
  TARGET_URN,
  TARGET_CASE_ID,
  TARGET_SAS_LINK_DOCUMENT_ID,
  SAS_LINK_DOMAIN,
} = Cypress.env()

let routes: ApiRoutes

describe("Simple Tracker", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("run a tracker through to all documents being indexed", () => {
    cy.clearCaseTracker(TARGET_URN, TARGET_CASE_ID)
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
      .api(
        routes.GET_DOCUMENT_SAS_LINK(
          TARGET_URN,
          TARGET_CASE_ID,
          TARGET_SAS_LINK_DOCUMENT_ID
        )
      )
      .its("body")
      .then((body) => {
        expect(body)
          .to.be.a("string")
          .and.satisfy((s) => s.startsWith(SAS_LINK_DOMAIN))
      })
  })
})
