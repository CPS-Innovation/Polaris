import { CorrelationId, correlationIds } from "../../support/correlation-ids"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"

const {
  HAPPY_PATH_URN,
  HAPPY_PATH_CASE_ID,
  HAPPY_PATH_DOCUMENT_ID,
  HAPPY_PATH_TARGET_SEARCH_TEXT,
} = Cypress.env()

const statusCodes = {
  BAD_REQUEST_400: 400,
  UNAUTHORIZED_401: 401,
  FORBIDDEN_403: 403,
}

const getRoutesToTest = (correlationId: CorrelationId, headers: any) => {
  const routeGenerators = makeApiRoutes(headers)

  const routesToTest = [
    routeGenerators.LIST_CASES(HAPPY_PATH_URN, correlationId),
    routeGenerators.GET_CASE(HAPPY_PATH_URN, HAPPY_PATH_CASE_ID, correlationId),
    routeGenerators.TRACKER_START(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      correlationId
    ),
    routeGenerators.GET_TRACKER(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      correlationId
    ),
    routeGenerators.GET_SEARCH(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_TARGET_SEARCH_TEXT,
      correlationId
    ),
    routeGenerators.GET_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      correlationId
    ),
    routeGenerators.CHECKOUT_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      correlationId
    ),
    routeGenerators.CANCEL_CHECKOUT_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      correlationId
    ),
    routeGenerators.SAVE_DOCUMENT(
      HAPPY_PATH_URN,
      HAPPY_PATH_CASE_ID,
      HAPPY_PATH_DOCUMENT_ID,
      correlationId
    ),
  ]

  return routesToTest
}

describe("Gateway endpoint auth failures", { tags: "@ci" }, () => {
  ;["EMPTY", "UNDEFINED", "NOT_A_GUID"].forEach(
    (correlationId: CorrelationId) => {
      it(`rejects calls that have a token but do not have an appropriate correlation id: (${correlationId} correlation id)`, () => {
        cy.getADTokens().then((adTokens) => {
          const routesToTest = getRoutesToTest(correlationId, {
            Authorization: `Bearer ${adTokens.access_token}`,
          })

          for (const route of routesToTest) {
            cy.log(`Testing ${route.headers["correlation-id"]}`)
            cy.api({
              ...route,
              failOnStatusCode: false,
            }).then((response) =>
              expect(response.status).to.equal(statusCodes.BAD_REQUEST_400)
            )
          }
        })
      })
    }
  )

  describe("Broken AD auth", () => {
    // Note: local tests and tests deployed in an environment will fail in a different way because
    //  deployed gateway will have AD auth enforced before the azure function gets to handle the request.
    //  Locally we will get a 400 as c# gets to process the request.  Deployed we receive a 302 as part of
    //  the AD auth redirect flow.
    //  In either case we can still usefully test to see if the requests fails or not.

    it("rejects calls that have a correlation id but do not have a token", () => {
      const routesToTest = getRoutesToTest("BLANK", {})

      for (const route of routesToTest) {
        cy.api({
          ...route,
          failOnStatusCode: false,
          followRedirect: false,
        }).then((response) => expect(response.isOkStatusCode).to.equal(false))
      }
    })

    it("rejects calls that have a correlation id but do not have a valid token", () => {
      cy.getADTokens().then((adTokens) => {
        var brokenToken = adTokens.access_token.slice(0, -1)

        const routesToTest = getRoutesToTest("BLANK", {
          Authorization: `Bearer ${brokenToken}`,
        })

        for (const route of routesToTest) {
          cy.api({
            ...route,
            failOnStatusCode: false,
            followRedirect: false,
          }).then((response) => expect(response.isOkStatusCode).to.equal(false))
        }
      })
    })
  })

  it("rejects calls that have a token and correlation id but do not have cms auth values", () => {
    cy.getADTokens().then((adTokens) => {
      const routesToTest = getRoutesToTest("BLANK", {
        Authorization: `Bearer ${adTokens.access_token}`,
      })

      for (const route of routesToTest) {
        cy.api({
          ...route,
          failOnStatusCode: false,
        }).then((response) =>
          expect(response.status).to.equal(statusCodes.FORBIDDEN_403)
        )
      }
    })
  })
})
