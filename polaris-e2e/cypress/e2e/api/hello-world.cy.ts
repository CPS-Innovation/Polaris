/// <reference types="cypress" />

const { TARGET_URN, API_ROOT_DOMAIN } = Cypress.env()

const getListCasesUrl = () => `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases`

const getCaseUrl = (caseId: number) =>
  `${API_ROOT_DOMAIN}/api/urns/${TARGET_URN}/cases/${caseId}`

describe("A hello world test", () => {
  it("can list case details and retrieve a case", () => {
    cy.getApiHeaders().then((headers) => {
      cy.api({
        url: getListCasesUrl(),
        headers,
      }).then(({ body }) =>
        cy.api<{ body: { id: number }[] }>({
          url: getCaseUrl(body[0].id),
          headers,
        })
      )
    })
  })

  it("can refresh a casew tracker and reload the case", () => {
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
