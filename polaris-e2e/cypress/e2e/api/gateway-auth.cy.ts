import { CorrelationId, correlationIds } from "../../support/correlation-ids"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"

const {
  HAPPY_PATH_URN,
  HAPPY_PATH_CASE_ID,
  HAPPY_PATH_DOCUMENT_ID,
  HAPPY_PATH_TARGET_SEARCH_TEXT,
} = Cypress.env()

const statusCodes = {
  UNAUTHORIZED_401: 401,
  BAD_REQUEST_400: 400,
}

const getRoutesToTest = (correlationId: CorrelationId) => {
  const routeGenerators = makeApiRoutes({})

  const routesToTest = [
    routeGenerators.LIST_CASES(HAPPY_PATH_URN, correlationId),
    // routeGenerators.GET_CASE(HAPPY_PATH_URN, HAPPY_PATH_CASE_ID, correlationId),
    // routeGenerators.TRACKER_START(
    //   HAPPY_PATH_URN,
    //   HAPPY_PATH_CASE_ID,
    //   correlationId
    // ),
    // routeGenerators.GET_TRACKER(
    //   HAPPY_PATH_URN,
    //   HAPPY_PATH_CASE_ID,
    //   correlationId
    // ),
    // routeGenerators.GET_SEARCH(
    //   HAPPY_PATH_URN,
    //   HAPPY_PATH_CASE_ID,
    //   HAPPY_PATH_TARGET_SEARCH_TEXT,
    //   correlationId
    // ),
    // routeGenerators.GET_DOCUMENT(
    //   HAPPY_PATH_URN,
    //   HAPPY_PATH_CASE_ID,
    //   HAPPY_PATH_DOCUMENT_ID,
    //   correlationId
    // ),
    // routeGenerators.CHECKOUT_DOCUMENT(
    //   HAPPY_PATH_URN,
    //   HAPPY_PATH_CASE_ID,
    //   HAPPY_PATH_DOCUMENT_ID,
    //   correlationId
    // ),
    // routeGenerators.CANCEL_CHECKOUT_DOCUMENT(
    //   HAPPY_PATH_URN,
    //   HAPPY_PATH_CASE_ID,
    //   HAPPY_PATH_DOCUMENT_ID,
    //   correlationId
    // ),
    // routeGenerators.SAVE_DOCUMENT(
    //   HAPPY_PATH_URN,
    //   HAPPY_PATH_CASE_ID,
    //   HAPPY_PATH_DOCUMENT_ID,
    //   correlationId
    // ),
  ]

  return routesToTest
}

describe("Gateway endpoint auth", { tags: "@ci" }, () => {
  it("rejects calls without correlation-id header", () => {
    const routesToTest = getRoutesToTest("UNDEFINED")

    for (const route of routesToTest) {
      cy.api({
        ...route,
        failOnStatusCode: false,
      }).then((response) =>
        expect(response.status).to.equal(statusCodes.BAD_REQUEST_400)
      )
    }
  })

  it("rejects calls that do not have AD auth", () => {
    const routesToTest = getRoutesToTest("BLANK")

    for (const route of routesToTest) {
      cy.api({
        ...route,
        failOnStatusCode: false,
      }).then((response) =>
        expect(response.status).to.equal(statusCodes.UNAUTHORIZED_401)
      )
    }
  })
})
