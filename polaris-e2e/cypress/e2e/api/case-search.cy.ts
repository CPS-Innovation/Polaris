/// <reference types="cypress" />
import "cypress-wait-until"
import { CaseDetails } from "../../../gateway/CaseDetails"
import { CaseSearchResult } from "../../../gateway/CaseSearchResult"
import { ApiRoutes, makeApiRoutes } from "./helpers/make-routes"

const { TARGET_URN, TARGET_CASE_ID } = Cypress.env()

let routes: ApiRoutes

describe("Case search and details", { tags: '@ci' }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("can list cases for a URN and then retrieve the first case of the URN", () => {
    cy.api<CaseSearchResult[]>(routes.LIST_CASES(TARGET_URN, "PHASE_1"))
      .api<CaseDetails>(routes.GET_CASE(TARGET_URN, TARGET_CASE_ID, "PHASE_1"))
      .then(({ body }) => {
        expect(body.uniqueReferenceNumber).to.equal(TARGET_URN)
      })
  })
})
