import "@testing-library/cypress/add-commands"
import { injectTokens } from "./inject-tokens"
import { CorrelationId, correlationIds } from "./correlation-ids"

declare global {
  namespace Cypress {
    interface Chainable<Subject> {
      safeLogEnvVars(): Chainable<any>
      loginToAD(): Chainable<any>
      loginToCms(): Chainable<any>
      preemptivelyAttachCookies(): Chainable<any>
      fullLogin(): Chainable<any>
      clearCaseTracker(urn: string, caseId: string): Chainable<any>
      requestToken(): Chainable<Response<{ access_token: string }>>
      getAuthHeaders(): Chainable<{
        Authorization: string
        "Correlation-Id": string
      }>
      setPolarisInstrumentationGuid(guid: string): Chainable<AUTWindow>

      // roll our own typing to help cast the body returned from the api call
      api<T>(url: string, body?: RequestBody): Chainable<ApiResponseBody<T>>
      api<T>(
        method: HttpMethod,
        url: string,
        body?: RequestBody
      ): Chainable<ApiResponseBody<T>>
      api<T>(options: Partial<RequestOptions>): Chainable<ApiResponseBody<T>>
    }
  }

  export interface ApiResponseBody<T> extends Cypress.Response<T> {
    size?: number
  }
}
const {
  AUTHORITY,
  CLIENTID,
  CLIENTSECRET,
  APISCOPE,
  AD_USERNAME,
  AD_PASSWORD,
  API_ROOT_DOMAIN,
  CMS_USERNAME,
  CMS_PASSWORD,
  CMS_LOGIN_PAGE_URL,
  CMS_FULL_COOKIE_LOGIN_PAGE_URL,
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

Cypress.Commands.add(
  "getAuthHeaders",
  {
    prevSubject: ["optional"],
  },
  () => {
    cy.request({
      followRedirect: false,
      method: "POST",
      url: CMS_FULL_COOKIE_LOGIN_PAGE_URL,
      form: true,
      body: {
        username: CMS_USERNAME,
        password: CMS_PASSWORD,
      },
    }).then(() => {
      cy.requestToken().then((response) => ({
        Authorization: `Bearer ${response.body.access_token}`,
        credentials: "include",
        //Cookie: response.headers["set-cookie"][0].split(";")[0],
      }))
    })
  }
)

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

Cypress.Commands.add("preemptivelyAttachCookies", () => {
  var args = {
    redirectUrl:
      API_ROOT_DOMAIN +
      "/polaris?polaris-ui-url=" +
      encodeURIComponent(Cypress.config().baseUrl + "?auth-refresh"),
  } as { redirectUrl: string }

  cy.origin(API_ROOT_DOMAIN, { args }, ({ redirectUrl }) => {
    cy.visit(redirectUrl)
  })
})

Cypress.Commands.add("fullLogin", () => {
  cy.loginToAD().loginToCms().preemptivelyAttachCookies()
})

Cypress.Commands.add("clearCaseTracker", (urn, caseId) => {
  cy.request({
    url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
    method: "DELETE",
    followRedirect: false,
    headers: {
      authorization: `Bearer ${cachedTokenResponse.access_token}`,
      "correlation-id": correlationIds.BLANK,
    },
  })
  cy.wait(1000)
})

declare global {
  var __POLARIS_INSTRUMENTATION_GUID__: string
}

Cypress.Commands.add(
  "setPolarisInstrumentationGuid",
  (correlationId: CorrelationId) =>
    // note: on any direct navigate ion using cy.visit() this setting will be lost
    cy.window().then((win) => {
      win.__POLARIS_INSTRUMENTATION_GUID__ = correlationIds[correlationId]
    })
)
