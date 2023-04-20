/// <reference types="cypress" />

const { TARGET_URN } = Cypress.env()

describe("Reauth flow", () => {
  it("can send the user around the reauth flow", () => {
    cy.on("uncaught:exception", () => false)

    // have we logged in OK?
    cy.loginToAD()
      .visit(`/polaris-ui/case-search-results?urn=${TARGET_URN}`)
      // we expect to not be logged-in to CMS...
      .contains("CMS_AUTH_ERROR")
      // ... and now we do log in
      .loginToCms()

    cy.visit(`/polaris-ui/case-search-results?urn=${TARGET_URN}`)

    // if we find the target URN we have made a successful round trip to DDEI
    cy.findByTestId(`link-${TARGET_URN}`)
  })
})

export {}
