/// <reference types="cypress" />
import "cypress-wait-until"
import { CaseDetails } from "../../../gateway/CaseDetails"
import { CaseSearchResult } from "../../../gateway/CaseSearchResult"
import { makeRoutes } from "./helpers/make-routes"

const { TARGET_URN, TARGET_CASE_ID } = Cypress.env()

let routes

describe("Case search and details", () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeRoutes(headers)
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
