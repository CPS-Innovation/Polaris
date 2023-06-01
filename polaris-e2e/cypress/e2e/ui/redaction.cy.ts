/// <reference types="cypress" />

const {
  TARGET_URN,
  TARGET_DOCUMENT_NAME,
  TARGET_CASE_ID,
  TARGET_NOT_OCR_PROCESSED_DOCUMENT_NAME,
} = Cypress.env()

describe("Redactions", () => {
  it("can only redact documents that are allowed", () => {
    cy.on("uncaught:exception", () => false)

    cy.fullLogin()

    cy.clearCaseTracker(TARGET_URN, TARGET_CASE_ID)
    cy.visit("/polaris-ui")

    cy.findByTestId("input-search-urn").type(`${TARGET_URN}{enter}`)
    cy.findByTestId(`link-${TARGET_URN}`).click()
    // open case details page
    cy.findByTestId("btn-accordion-open-close-all").click()

    // this document has ocrProcessed = "Y" in the mock
    cy.findByText(TARGET_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("Procedure")
    cy.findByTestId("btn-redact")

    // this document has ocrProcessed = null in the mock
    cy.findByText(TARGET_NOT_OCR_PROCESSED_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("Dummy")
    cy.findByTestId("redaction-warning")
  })
})

export {}
