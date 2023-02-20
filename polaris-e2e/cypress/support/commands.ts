import "@testing-library/cypress/add-commands"
import { injectTokens } from "./inject-tokens"

declare global {
  namespace Cypress {
    interface Chainable<Subject> {
      safeLogEnvVars(): Chainable<any>
      loginToAD(): Chainable<any>
      loginToCms(): Chainable<any>
      requestToken(): Chainable<Response<unknown>>
    }
  }
}

const {
  AUTHORITY,
  CLIENTID,
  CLIENTSECRET,
  APISCOPE,
  AD_USERNAME,
  AD_PASSWORD,
  CMS_USERNAME,
  CMS_PASSWORD,
  CMS_LOGIN_PAGE_URL,
  CMS_USERNAME_FIELD_LOCATOR,
  CMS_PASSWORD_FIELD_LOCATOR,
  CMS_SUBMIT_BUTTON_LOCATOR,
  CMS_LOGGED_IN_CONFIRMATION_LOCATOR,
} = Cypress.env()

const AUTOMATION_LANDING_PAGE_URL = "/?automation-test-first-visit=true"

let cachedTokenExpiryTime = new Date().getTime()
let cachedTokenResponse = null

Cypress.Commands.add("safeLogEnvVars", () => {
  const processSecret = (secret: string) => {
    const chars = (secret || "").split("")
    const length = chars.length
    if (!length) {
      return "IS EMPTY!!!!"
    }
    const [initial] = chars

    return `'${initial}' plus ${length - 1} chars`
  }

  const rawEnvVars = { ...Cypress.env() }
  const processedEnvVars = {
    ...rawEnvVars,
    CLIENTSECRET: processSecret(rawEnvVars.CLIENTSECRET),
    AD_PASSWORD: processSecret(rawEnvVars.AD_PASSWORD),
    CMS_PASSWORD: processSecret(rawEnvVars.CMS_PASSWORD),
  }

  cy.log(JSON.stringify(processedEnvVars))
})

Cypress.Commands.add("loginToAD", () => {
  let getToken =
    cachedTokenResponse && cachedTokenExpiryTime > new Date().getTime()
      ? cy.visit(AUTOMATION_LANDING_PAGE_URL).then(() => ({
          body: cachedTokenResponse,
        }))
      : cy.visit(AUTOMATION_LANDING_PAGE_URL).requestToken()

  return getToken
    .then((response) => {
      injectTokens(response.body)
      cachedTokenResponse = response.body
      cachedTokenExpiryTime = new Date().getTime() + 20 * 60 * 1000 // 20 mins ok for a start?
    })
    .reload()
    .visit("/")
    .contains(AD_USERNAME)
})

Cypress.Commands.add("loginToCms", () => {
  // Pass in dependencies via args option
  const args = {
    username: CMS_USERNAME,
    password: CMS_PASSWORD,
    usernameFieldLocator: CMS_USERNAME_FIELD_LOCATOR,
    passwordFieldLocator: CMS_PASSWORD_FIELD_LOCATOR,
    submitButtonLocator: CMS_SUBMIT_BUTTON_LOCATOR,
    loggedInConfirmationLocator: CMS_LOGGED_IN_CONFIRMATION_LOCATOR,
  } as {
    username: string
    password: string
    usernameFieldLocator: string
    passwordFieldLocator: string
    submitButtonLocator: string
    loggedInConfirmationLocator: string
  }

  cy.origin(
    CMS_LOGIN_PAGE_URL,
    { args },
    ({
      username,
      password,
      usernameFieldLocator,
      passwordFieldLocator,
      submitButtonLocator,
      loggedInConfirmationLocator,
    }) => {
      cy.visit("/")
      cy.get(usernameFieldLocator).type(username)
      cy.get(passwordFieldLocator).type(password)
      cy.get(submitButtonLocator).click()
      cy.get(loggedInConfirmationLocator).should("exist")
    }
  )
})

Cypress.Commands.add(
  "requestToken",
  {
    prevSubject: ["optional"],
  },
  () =>
    cy.request({
      url: AUTHORITY + "/oauth2/v2.0/token",
      method: "POST",
      body: {
        grant_type: "password",
        client_id: CLIENTID,
        client_secret: CLIENTSECRET,
        scope: ["openid profile"].concat([APISCOPE]).join(" "),
        username: AD_USERNAME,
        password: AD_PASSWORD,
      },
      form: true,
    })
)
