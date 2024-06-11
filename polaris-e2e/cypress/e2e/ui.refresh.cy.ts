const {
  REFRESH_UI_TARGET_URN,
  REFRESH_UI_TARGET_CASE_ID,
  TARGET_PEOPLE_DOC_NAME,
} = Cypress.env()

describe(
  "Refresh via guid-controlled ",
  { tags: ["@ci", "@ci-chunk-1"] },
  () => {
    it("can update a document", () => {
      cy.fullLogin()

      cy.clearCaseTracker(REFRESH_UI_TARGET_URN, REFRESH_UI_TARGET_CASE_ID)

      cy.visit("/polaris-ui/case-search?redactionLog=false")
      cy.setPolarisInstrumentationGuid("PHASE_1")

      cy.findByTestId("input-search-urn").type(
        `${REFRESH_UI_TARGET_URN}{enter}`
      )
      cy.findByTestId(`link-${REFRESH_UI_TARGET_URN}`).click()

      cy.findByTestId("btn-accordion-open-close-all").click()
      cy.findByText(TARGET_PEOPLE_DOC_NAME).click()

      // our expected phase 1 text is present
      cy.selectPDFTextElement("Alice")

      cy.setPolarisInstrumentationGuid("PHASE_2")
      cy.findByTestId("btn-redact").click({ force: true })

      cy.findByTestId("btn-save-redaction-0").click(
        // The user cannot submit a redaction until the current pipeline refresh is complete.
        //  In this instance, we forgive a long running process
        { timeout: 5 * 60 * 1000 }
      )

      // wait for the spinner to appear and disappear
      cy.findByTestId("pdfTab-spinner-0").should("exist")
      cy.findByTestId("pdfTab-spinner-0").should("not.exist")
      cy.findByTestId("div-pdfviewer-0").should("exist")

      // our expected phase 2 text is present, the document has been refreshed in the tracker
      cy.selectPDFTextElement("Bob")
    })
  }
)

export {}
