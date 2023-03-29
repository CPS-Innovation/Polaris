/// <reference types="cypress" />

const {
  AD_USERNAME,
  TARGET_URN,
  TARGET_DEFENDANT_NAME,
  TARGET_DOCUMENT_NAME,
  TARGET_DOCUMENT_TEXT_FRAGMENT,
  TARGET_SEARCH_TEXT,
  TARGET_CASE_ID,
} = Cypress.env()

describe("Happy Path", () => {
  it("can view a case", () => {
    cy.on("uncaught:exception", () => false)

    cy.fullLogin()
    cy.clearCaseTracker(TARGET_URN, TARGET_CASE_ID)
    cy.visit("/")
    cy.findByTestId("input-search-urn").type(`${TARGET_URN}{enter}`)

    // // open case details page
    cy.findByTestId(`link-${TARGET_URN}`).click()

    // is our defendant correct
    cy.findByTestId("txt-defendant-name").contains(TARGET_DEFENDANT_NAME)

    // open our target document
    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.findByText(TARGET_DOCUMENT_NAME).click()

    cy.findByTestId("btn-open-pdf").contains(TARGET_DOCUMENT_NAME)
    cy.get("span").contains(TARGET_DOCUMENT_TEXT_FRAGMENT)

    cy.findByTestId("input-search-case").type(`${TARGET_SEARCH_TEXT}{enter}`)
    // cy.findByTestId(`link-result-document-${TARGET_DOCUMENT_ID}`).click()

    // cy.findByTestId("tabs").contains(
    //   `for "${TARGET_SEARCH_TEXT}" in ${TARGET_DOCUMENT_NAME}`
    // )
  })
})

export {}
