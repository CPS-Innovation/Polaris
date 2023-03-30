/// <reference types="cypress" />

const { TARGET_URN, API_ROOT_DOMAIN } = Cypress.env()

describe("A hello world test", () => {
  it("can retrieve case details", function () {
    cy.getApiHeaders().then((headers) => {
      cy.api({
        url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases`,
        headers,
      }).then((response) =>
        cy.api({
          url: `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${response.body[0].id}`,
          headers,
        })
      )
    })
  })
})

export {}
