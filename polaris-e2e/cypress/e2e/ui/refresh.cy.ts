const { REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID, TARGET_NUMBERS_DOC_NAME } =
  Cypress.env()

describe("Refresh via guid-controlled ", () => {
  it("can update a document", () => {
    cy.fullLogin()

    cy.clearCaseTracker(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID)

    cy.visit("/polaris-ui")
    cy.setPolarisInstrumentationGuid("PHASE_1")
    cy.findByTestId("input-search-urn").type(`${REFRESH_TARGET_URN}{enter}`)
    cy.findByTestId(`link-${REFRESH_TARGET_URN}`).click()

    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.findByText(TARGET_NUMBERS_DOC_NAME).click()

    cy.selectPDFTextElement("Three")

    cy.setPolarisInstrumentationGuid("PHASE_2")
    cy.findByTestId("btn-redact").should("be.disabled")
    cy.findByTestId("select-redaction-type").should("have.length", 1)
    cy.findByTestId("select-redaction-type").select("2")
    cy.findByTestId("btn-redact").click({ force: true })

    cy.findByTestId("btn-save-redaction-0").click(
      // The user cannot submit a redaction until the current pipeline refresh is complete.
      //  In this instance, we forgive a long running process
      { timeout: 5 * 60 * 1000 }
    )

    // we need to add redaction log modal functionality here
    // cy.findByTestId("pdfTab-spinner-0").should("exist")
    // cy.findByTestId("pdfTab-spinner-0").should("not.exist")
    // cy.findByTestId("div-pdfviewer-0").should("exist")
    // cy.selectPDFTextElement("Four")
  })
})

export {}
