/// <reference types="cypress" />
import "cypress-wait-until";
import { PipelineResults } from "../../gateway/PipelineResults";
import { ApiRoutes, makeApiRoutes } from "../support/helpers/make-routes";
import { WAIT_UNTIL_OPTIONS } from "../support/options";

const {
  ATTACHMENT_TARGET_URN,
  ATTACHMENT_CASE_ID,
  ATTACHMENT_PARENT_DOCUMENT_ID,
} = Cypress.env();

let routes: ApiRoutes;

describe("Attachments", { tags: ["@ci", "@ci-chunk-3"] }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers);
    });
  });

  xit("can observe expected attachment documents and parents in tracker responses", () => {
    cy.clearCaseTracker(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID)
      .api(routes.TRACKER_START(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID))

      .waitUntil(
        () =>
          cy
            .api<PipelineResults>({
              ...routes.GET_TRACKER(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID),
              failOnStatusCode: false,
            })
            .its("body")
            .then(({ documents }) => !!documents?.length),
        WAIT_UNTIL_OPTIONS
      )
      .api<PipelineResults>(
        routes.GET_TRACKER(ATTACHMENT_TARGET_URN, ATTACHMENT_CASE_ID)
      )
      .its("body")
      .then(({ documents }) => {
        expect(
          documents.some(
            (document) => document.documentId == ATTACHMENT_PARENT_DOCUMENT_ID
          )
        ).to.be.true;
      });
  });
});
