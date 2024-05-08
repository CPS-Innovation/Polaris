/// <reference types="cypress" />

const {
  ATTACHMENT_FAILURE_CASE_URN,
  ATTACHMENT_FAILURE_CASE_ID,
  ATTACHMENT_FAILURE_DOCUMENT_ID,
} = Cypress.env()

describe("Attachment failure", { tags: '@ci' }, () => {
  it("can display attachment failure message", () => {
    cy.on("uncaught:exception", () => false)

    cy.fullLogin()
    cy.clearCaseTracker(ATTACHMENT_FAILURE_CASE_URN, ATTACHMENT_FAILURE_CASE_ID)

    cy.visit("/polaris-ui")
    cy.findByTestId("input-search-urn").type(`${ATTACHMENT_FAILURE_CASE_URN}{enter}`)
    cy.findByTestId(`link-${ATTACHMENT_FAILURE_CASE_URN}`).click()

    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.get("#side-panel").scrollTo("bottom");
    cy.findByTestId(`link-document-${ATTACHMENT_FAILURE_DOCUMENT_ID}`)

    // We find all expected indicators, in the order we expect them
    cy.get(`[data-testid^="failed-attachment-warning-${ATTACHMENT_FAILURE_DOCUMENT_ID}"]`).should("be.visible")
  })
})
