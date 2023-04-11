import { correlationIds } from "../../support/correlation-ids"

const { REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID } = Cypress.env()

describe("Refresh via guid-controlled ", () => {
  it("can update a document", () => {
    cy.fullLogin()

    cy.clearCaseTracker(REFRESH_TARGET_URN, REFRESH_TARGET_CASE_ID)

    cy.visit("/")
    cy.setPolarisInstrumentationGuid("PHASE_1")
    cy.findByTestId("input-search-urn").type(`${REFRESH_TARGET_URN}{enter}`)
    cy.findByTestId(`link-${REFRESH_TARGET_URN}`).click()

    cy.findByTestId("input-search-case").type(`four{enter}`)
    cy.findByTestId("btn-modal-close")
    cy.get("#modal").contains("Four").should("not.exist")
    cy.findByTestId("btn-modal-close").click()

    cy.findByTestId("input-search-case").clear().type(`three{enter}`)
    cy.findByTestId("btn-modal-close")
    cy.get("#modal").contains("Three")
    cy.get("#modal").findByText("e2e-numbers-pre").click()

    
  })
})

export {}
