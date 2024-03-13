/// <reference types="cypress" />
export const loginViaAD = (username: string, password: string) => {
  cy.visit(`${Cypress.config().baseUrl}/polaris-ui`)
  cy.task("log", `Msal login start`)
  cy.origin(
    "login.microsoftonline.com",
    {
      args: {
        username,
      },
    },
    ({ username }) => {
      cy.get('input[type="email"]').type(username, {
        log: false,
      })
      cy.task("log", `Msal login with username:${username}`)
      cy.get('input[type="submit"]').click()
    }
  )

  cy.origin(
    "login.microsoftonline.com",
    {
      args: {
        password,
      },
    },
    ({ password }) => {
      cy.get('input[type="password"]').type(password, {
        log: false,
      })
      cy.get('input[type="submit"]').click()
    }
  )

  cy.origin(
    "login.microsoftonline.com",
    {
      args: {},
    },
    () => {
      cy.get('input[type="submit"]')
      cy.get("#idBtn_Back").click()
    }
  )

  // Ensure Microsoft has redirected us back to the sample app with our logged in user.
  cy.url().should("equal", `${Cypress.config().baseUrl}/polaris-ui`)

  return cy.visit("/polaris-ui").contains(username)
}
