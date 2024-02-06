/// <reference types="cypress" />

const {
  WITNESS_TARGET_URN,
  WITNESS_TARGET_CASE_ID,
  WITNESS_DOCUMENT_ID,
  WITNESS_EXPECTED_INDICATORS,
  WITNESS_NOT_EXPECTED_INDICATORS,
} = Cypress.env()

describe("Witness Indicators", { tags: '@ci' }, () => {
  it("can display witness indicators", () => {
    cy.on("uncaught:exception", () => false)

    cy.fullLogin()
    cy.clearCaseTracker(WITNESS_TARGET_URN, WITNESS_TARGET_CASE_ID)

    cy.visit("/polaris-ui")
    cy.findByTestId("input-search-urn").type(`${WITNESS_TARGET_URN}{enter}`)
    cy.findByTestId(`link-${WITNESS_TARGET_URN}`).click()

    cy.findByTestId("btn-accordion-open-close-all").click()
    cy.findByTestId(`link-document-${WITNESS_DOCUMENT_ID}`)

    // We find all expected indicators, in the order we expect them
    cy.get(`[data-testid^="indicator-${WITNESS_DOCUMENT_ID}-"]`)
      .should("have.length", 10)
      .then((indicators) => {
        const indicatorCodesInOrder = indicators
          .map((id) => indicators[id].getAttribute("data-testid").split("-")[3])
          .toArray()
          .join(",")

        expect(indicatorCodesInOrder).equals(WITNESS_EXPECTED_INDICATORS)
      })

    // Check that indicators not expected are not there
    ;(WITNESS_NOT_EXPECTED_INDICATORS as string)
      .split(",")
      .forEach((indicator) => {
        cy.findByTestId(`indicator-${WITNESS_DOCUMENT_ID}-${indicator}`).should(
          "not.exist"
        )
      })
  })
})
