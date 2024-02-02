/// <reference types="cypress" />
import "cypress-wait-until"
import { PipelineResults } from "../../../gateway/PipelineResults"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"
import { WAIT_UNTIL_OPTIONS } from "../../support/options"

const {
  ATTACHMENT_TARGET_URN,
  ATTACHMENT_CASE_ID,
  ATTACHMENT_PARENT_DOCUMENT_ID,
} = Cypress.env()

let routes: ApiRoutes

describe("Attachments", { tags: "@ci" }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("can observe expected attachment documents and parents in tracker responses", () => {
    cy.clearCaseTracker(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID)
      .api(routes.TRACKER_START(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID))
      .waitUntil(
        () =>
          cy
            .api({
              ...routes.GET_TRACKER(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID),
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
              routes.GET_TRACKER(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID)
            )
            .its("body")
            .then(({ documents }) => !!documents.length),
        WAIT_UNTIL_OPTIONS
      )
      .api<PipelineResults>(
        routes.GET_TRACKER(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID)
      )
      .its("body")
      .then(({ documents }) => {
        expect(
          documents.some(
            (document) =>
              document.cmsDocType.documentCategory === "Attachment" &&
              document.polarisParentDocumentId ==
                ATTACHMENT_PARENT_DOCUMENT_ID &&
              document.presentationFlags.write == "AttachmentCategoryNotAllowed"
          )
        ).to.be.true
        expect(
          documents.some(
            (document) =>
              document.polarisDocumentId == ATTACHMENT_PARENT_DOCUMENT_ID
          )
        ).to.be.true
      })
  })
})
