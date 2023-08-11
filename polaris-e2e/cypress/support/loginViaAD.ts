/// <reference types="cypress" />

export const loginViaAD = (username: string, password: string) => {
  cy.visit("http://localhost:3000/polaris-ui")
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
  cy.url().should("equal", "http://localhost:3000/polaris-ui")

  cy.origin("login.microsoftonline.com", { args: "" }, () => {
    //NOTE : this wait is needed otherwise e2e test get stuck on the "Pick an Account screen of msal"
    cy.wait(1000)
  })
  return cy.visit("/polaris-ui").contains(username)
}
