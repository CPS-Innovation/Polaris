/// <reference types="cypress" />

const {
  DOC_TOGGLE_TARGET_URN,
  DOC_TOGGLE_TARGET_CASE_ID,
  TARGET_NOT_OCR_PROCESSED_DOCUMENT_NAME,
  TARGET_NOT_CORRECT_DOC_TYPE_DOCUMENT_NAME,
  TARGET_CAN_REDACT_DOCUMENT_NAME,
  TARGET_ALREADY_CHECKED_OUT_DOCUMENT_NAME,
  TARGET_DIRECTION_OUT_DOCUMENT_NAME,
} = Cypress.env();

describe("Document toggle", { tags: '@ci' }, () => {
  it("can only redact documents that are allowed", () => {
    cy.on("uncaught:exception", () => false)

    cy.fullLogin()

    cy.clearCaseTracker(DOC_TOGGLE_TARGET_URN, DOC_TOGGLE_TARGET_CASE_ID);
    cy.visit("/polaris-ui")
    cy.setPolarisInstrumentationGuid("PHASE_1")
    cy.findByTestId("input-search-urn").type(`${DOC_TOGGLE_TARGET_URN}{enter}`);
    cy.findByTestId(`link-${DOC_TOGGLE_TARGET_URN}`).click();
    // open case details page
    cy.findByTestId("btn-accordion-open-close-all").click()

    cy.findByText(TARGET_CAN_REDACT_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("12345")
    cy.findByTestId("btn-redact")
    cy.findByTestId("tab-remove").click()

    cy.findByText(TARGET_NOT_OCR_PROCESSED_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("12345")
    cy.findByTestId("redaction-warning")
    cy.findByTestId("tab-remove").click()

    cy.findByText(TARGET_NOT_CORRECT_DOC_TYPE_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("12345")
    cy.findByTestId("redaction-warning")
    cy.findByTestId("tab-remove").click()

    cy.findByText(TARGET_DIRECTION_OUT_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("12345")
    cy.findByTestId("redaction-warning")
    cy.findByTestId("tab-remove").click()

    // helpful if this goes last as we don't have to tidy up the UI
    cy.findByText(TARGET_ALREADY_CHECKED_OUT_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("12345")
    cy.findByTestId("btn-redact").click()
    cy.findByTestId("btn-error-modal-ok")
    cy.contains("Mock OtherUser")
  })
})

export {}
