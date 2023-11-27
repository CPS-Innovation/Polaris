/// <reference types="cypress" />

const {
  REFRESH_TARGET_URN,
  REFRESH_TARGET_CASE_ID,
  TARGET_NOT_OCR_PROCESSED_DOCUMENT_NAME,
  TARGET_NOT_CORRECT_DOC_TYPE_DOCUMENT_NAME,
  TARGET_CAN_REDACT_DOCUMENT_NAME,
  TARGET_ALREADY_CHECKED_OUT_DOCUMENT_NAME,
  TARGET_DIRECTION_OUT_DOCUMENT_NAME,
} = Cypress.env()

describe("Document toggle", () => {
  it("can only redact documents that are allowed", () => {
    cy.on("uncaught:exception", () => false)

    cy.fullLogin()

    cy.clearCaseTracker(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID)
    cy.visit("/polaris-ui")
    cy.setPolarisInstrumentationGuid("PHASE_1")
    cy.findByTestId("input-search-urn").type(`${REFRESH_TARGET_URN}{enter}`)
    cy.findByTestId(`link-${REFRESH_TARGET_URN}`).click()
    // open case details page
    cy.findByTestId("btn-accordion-open-close-all").click()

    // cy.findByText(TARGET_CAN_REDACT_DOCUMENT_NAME).click()
    // cy.selectPDFTextElement("12345")
    // cy.findByTestId("btn-redact")
    // cy.findByTestId("tab-remove").click()

    // cy.findByText(TARGET_NOT_OCR_PROCESSED_DOCUMENT_NAME).click()
    // cy.selectPDFTextElement("12345")
    // cy.findByTestId("redaction-warning")
    // cy.findByTestId("tab-remove").click()

    // cy.findByText(TARGET_NOT_CORRECT_DOC_TYPE_DOCUMENT_NAME).click()
    // cy.selectPDFTextElement("12345")
    // cy.findByTestId("redaction-warning")
    // cy.findByTestId("tab-remove").click()

    // cy.findByText(TARGET_ALREADY_CHECKED_OUT_DOCUMENT_NAME).click()
    // cy.selectPDFTextElement("12345")
    // cy.findByTestId("btn-redact").click()
    // cy.findByTestId("btn-error-modal-ok")
    // cy.contains("Mock OtherUser")

    cy.findByText(TARGET_DIRECTION_OUT_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("12345")
    cy.findByTestId("redaction-warning")
    cy.findByTestId("tab-remove").click()
  })
})

export {}
