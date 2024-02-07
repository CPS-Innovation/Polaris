/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"

const { 
  ATTACHMENT_FAILURE_CASE_URN, 
  ATTACHMENT_FAILURE_CASE_ID,
  ATTACHMENT_FAILURE_PASSWORD_PROTECTED_DOCUMENT_NAME,
  ATTACHMENT_FAILURE_INVALID_EXTENSION_NAME,
  ATTACHMENT_FAILURE_OK_DOCUMENT_NAME
} =
  Cypress.env()

let routes: ApiRoutes

describe("Attachments failure", { tags: '@ci' }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("can observe failed documents that are UnableToConvertToPdf in the tracker responses ", () => {
    cy.clearCaseTracker(ATTACHMENT_FAILURE_CASE_URN, ATTACHMENT_FAILURE_CASE_ID)
      .api(routes.TRACKER_START(ATTACHMENT_FAILURE_CASE_URN, ATTACHMENT_FAILURE_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api({
              ...routes.GET_TRACKER(ATTACHMENT_FAILURE_CASE_URN, ATTACHMENT_FAILURE_CASE_ID),
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
              routes.GET_TRACKER(ATTACHMENT_FAILURE_CASE_URN, ATTACHMENT_FAILURE_CASE_ID)
            )
            .its("body")
            .then(({ documents }) => !!documents.length),
        WAIT_UNTIL_OPTIONS
      )
      .api<PipelineResults>(
        routes.GET_TRACKER(ATTACHMENT_FAILURE_CASE_URN, ATTACHMENT_FAILURE_CASE_ID)
      )
      .its("body")
      .then(({ documents }) => {
        expect(
          documents.some(
            (document) =>
              document.cmsOriginalFileName === ATTACHMENT_FAILURE_PASSWORD_PROTECTED_DOCUMENT_NAME &&
              document.status === "UnableToConvertToPdf"
          )
        ).to.be.true
        expect(
          documents.some(
            (document) =>
              document.cmsOriginalFileName === ATTACHMENT_FAILURE_INVALID_EXTENSION_NAME &&
              document.status === "UnableToConvertToPdf"
          )
        ).to.be.true
        expect(
          documents.some(
            (document) =>
              document.cmsOriginalFileName === ATTACHMENT_FAILURE_OK_DOCUMENT_NAME &&
              document.status === "Indexed"
          )
        ).to.be.true
      })
  })
})