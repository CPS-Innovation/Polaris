/// <reference types="cypress" />

const {
  AD_USERNAME,
  TARGET_URN,
  TARGET_DEFENDANT_NAME,
  TARGET_DOCUMENT_ID,
  TARGET_DOCUMENT_NAME,
  TARGET_DOCUMENT_TEXT_FRAGMENT,
  TARGET_SEARCH_TEXT,
} = Cypress.env()

describe("Polaris", () => {
  it("can view a case", () => {
    cy.on("uncaught:exception", () => false)

    cy.safeLogEnvVars()

    // have we logged in OK?
    cy.loginToAD()
      .visit(`/case-search-results?urn=${TARGET_URN}`)
      // we expect to not be logged-in to CMS...
      .contains("CMS_AUTH_ERROR")
      // ... and now we do log in
      .loginToCms()

    cy.visit("/")
    cy.findByTestId("input-search-urn").type(`${TARGET_URN}{enter}`)

    // // open case details page
    cy.findByTestId(`link-${TARGET_URN}`).click()

    // // is our defendant correct
    cy.findByTestId("txt-defendant-name").contains(TARGET_DEFENDANT_NAME)

    // open our target document
    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.findByTestId(`link-document-${TARGET_DOCUMENT_ID}`).click()

    cy.findByTestId("btn-open-pdf").contains(TARGET_DOCUMENT_NAME)
    cy.get("span").contains(TARGET_DOCUMENT_TEXT_FRAGMENT)

    cy.findByTestId("input-search-case").type(`${TARGET_SEARCH_TEXT}{enter}`)
    cy.findByTestId(`link-result-document-${TARGET_DOCUMENT_ID}`).click()

    cy.findByTestId("tabs").contains(
      `for "${TARGET_SEARCH_TEXT}" in ${TARGET_DOCUMENT_NAME}`
    )
  })
})
