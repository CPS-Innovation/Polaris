/// <reference types="cypress" />

const { HAPPY_PATH_URN, HAPPY_PATH_TARGET_DOCUMENT_NAME, HAPPY_PATH_CASE_ID } =
  Cypress.env()

describe("Redaction Log Feature", { tags: "@ci" }, () => {
  it("verify that under/over redaction log feature is available ", () => {
    cy.fullLogin()

    cy.clearCaseTracker(HAPPY_PATH_URN, HAPPY_PATH_CASE_ID)

    cy.visit(
      `polaris-ui/case-details/${HAPPY_PATH_URN}/${HAPPY_PATH_CASE_ID}?redactionLog=true`
    )
    cy.setPolarisInstrumentationGuid("PHASE_1")

    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.findByText(HAPPY_PATH_TARGET_DOCUMENT_NAME).click()
    cy.findByTestId("document-actions-dropdown-0").click()
    cy.findByTestId("dropdown-panel")
      .contains("Log an Under/Over redaction")
      .click()
    cy.findByTestId("div-modal").should("have.length", 1)
    cy.findByTestId("rl-under-over-redaction-content").should("be.visible")
  })

  it("verify that under redaction log feature is available", () => {
    cy.fullLogin()

    cy.clearCaseTracker(HAPPY_PATH_URN, HAPPY_PATH_CASE_ID)

    cy.visit(
      `polaris-ui/case-details/${HAPPY_PATH_URN}/${HAPPY_PATH_CASE_ID}?redactionLog=true`
    )
    cy.setPolarisInstrumentationGuid("PHASE_1")

    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.findByText(HAPPY_PATH_TARGET_DOCUMENT_NAME).click()
    cy.selectPDFTextElement("Multi Media Evidence")
    cy.findByTestId("btn-redact").should("be.disabled")
    cy.findByTestId("select-redaction-type").should("have.length", 1)
    cy.findByTestId("select-redaction-type").select("2")
    cy.findByTestId("btn-redact").should("not.be.disabled")
  })
})

export {}
