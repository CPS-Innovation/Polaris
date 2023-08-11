import "@testing-library/cypress/add-commands"
import { loginViaAD } from "./loginViaAD"
import { CorrelationId, correlationIds } from "./correlation-ids"
import { WAIT_UNTIL_OPTIONS } from "./options"
import "cypress-wait-until"

type ADTokens = {
  access_token: string
  id_token: string
  expires_in: string
  ext_expires_in: string
}

declare global {
  namespace Cypress {
    interface Chainable<Subject> {
      loginToAD(): Chainable<any>
      loginToCms(): Chainable<any>
      preemptivelyAttachCookies(): Chainable<any>
      fullLogin(): Chainable<any>
      clearCaseTracker(urn: string, caseId: string): Chainable<any>
      getADTokens(): Chainable<ADTokens>
      getAuthHeaders(): Chainable<{
        Authorization: string
        credentials: "include"
      }>
      setPolarisInstrumentationGuid(
        correlationId: CorrelationId
      ): Chainable<AUTWindow>
      selectPDFTextElement(matchString: string): void
      // roll our own typing to help cast the body returned from the api call
      checkDependencies(): Chainable<any>
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
  COOKIE_REDIRECT_URL,
  DEPENDENCIES,
} = Cypress.env()

const AUTOMATION_LANDING_PAGE_PATH =
  "/polaris-ui/?automation-test-first-visit=true"

Cypress.Commands.add(
  "getADTokens",
  {
    prevSubject: ["optional"],
  },
  () => {
    cy.task("retrieveTokenResponseFromNode").then((cachedTokens) => {
      if (cachedTokens) {
        return cachedTokens
      } else {
        return cy
          .log("setting tokens")
          .request({
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
          .then((response) =>
            cy
              .task("storeTokenResponseInNode", response.body)
              .task("retrieveTokenResponseFromNode")
          )
      }
    })
  }
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
      cy.getADTokens().then((adTokens) => ({
        Authorization: `Bearer ${adTokens.access_token}`,
        credentials: "include",
      }))
    })
  }
)

Cypress.Commands.add("loginToAD", () => {
  cy.session(
    `aad-${AD_USERNAME}`,
    () => {
      return loginViaAD(AD_USERNAME, AD_PASSWORD)
    },
    {
      validate() {
        cy.visit("/polaris-ui").contains(AD_USERNAME)
      },
      cacheAcrossSpecs: true,
    }
  )
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

  if (isFullUrl(CMS_LOGIN_PAGE_URL)) {
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
  } else {
    cy.reload()
    cy.visit(CMS_LOGIN_PAGE_URL)
    cy.get(args.usernameFieldLocator).type(args.username)
    cy.get(args.passwordFieldLocator).type(args.password)
    cy.get(args.submitButtonLocator).click()
    cy.get(args.loggedInConfirmationLocator).should("exist")
  }
})

Cypress.Commands.add("preemptivelyAttachCookies", () => {
  cy.visit(
    COOKIE_REDIRECT_URL +
      encodeURIComponent(Cypress.config().baseUrl + "/polaris-ui?auth-refresh")
  )
})

Cypress.Commands.add("fullLogin", () => {
  cy.loginToAD().loginToCms().preemptivelyAttachCookies()
})

Cypress.Commands.add("clearCaseTracker", (urn, caseId) => {
  cy.getADTokens().then((adTokens) => {
    cy.request({
      url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}`,
      method: "DELETE",
      followRedirect: false,
      headers: {
        authorization: `Bearer ${adTokens.access_token}`,
        "correlation-id": correlationIds.BLANK,
      },
      timeout: 5 * 60 * 1000,
    }).waitUntil(
      () =>
        cy
          .request({
            url: `${API_ROOT_DOMAIN}/api/urns/${urn}/cases/${caseId}/tracker`,
            failOnStatusCode: false,
            headers: {
              authorization: `Bearer ${adTokens.access_token}`,
              "correlation-id": correlationIds.BLANK,
            },
          })
          .its("status")
          .then((status) => status === 404),
      WAIT_UNTIL_OPTIONS
    )
  })
})

Cypress.Commands.add("checkDependencies", () => {
  for (var dependency of DEPENDENCIES) {
    const url = dependency
    const t0 = performance.now()
    cy.request({ url }).then(({ status, body }) => {
      var t1 = performance.now()
      cy.log(
        `Previous call to ${url} returned ${status} in ${(
          (t1 - t0) /
          1000
        ).toFixed(2)}s`
      )
    })
  }
})

declare global {
  var __POLARIS_INSTRUMENTATION_GUID__: string
}

Cypress.Commands.add(
  "setPolarisInstrumentationGuid",
  (correlationId: CorrelationId) =>
    // note: on any direct navigation using cy.visit() this setting will be lost
    //  as the page is reloaded.
    cy.window().then((win) => {
      win.__POLARIS_INSTRUMENTATION_GUID__ = correlationIds[correlationId]
      console.log(win.__POLARIS_INSTRUMENTATION_GUID__)
    })
)

Cypress.Commands.add("selectPDFTextElement", (matchString: string) => {
  cy.wait(100)
  cy.get(`.textLayer span:contains(${matchString})`)
    .filter(":not(:has(*))")
    .first()
    .then((element) => {
      cy.wrap(element)
        .trigger("mousedown")
        .then(() => {
          const el = element[0]
          const document = el.ownerDocument
          const range = document.createRange()
          range.selectNodeContents(el)
          document.getSelection()?.removeAllRanges()
          document.getSelection()?.addRange(range)
        })
        .trigger("mouseup")
      cy.document().trigger("selectionchange")
    })
})

const isFullUrl = (url: string) => url.startsWith("http")
