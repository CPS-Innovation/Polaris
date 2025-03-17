/// <reference types="cypress" />
import "cypress-wait-until"
import { ApiRoutes, makeApiRoutes } from "../support/helpers/make-routes"

const { DELETE_PAGE_TARGET_URN, DELETE_PAGE_CASE_ID, DELETE_PAGE_DOCUMENT_ID, DELETE_PAGE_VERSION_ID } = Cypress.env()

let routes: ApiRoutes

describe("Delete Page", { tags: ["@ci", "@ci-chunk-4"] }, () => {
  beforeEach(() => {
    cy.getAuthHeaders().then((headers) => {
      routes = makeApiRoutes(headers)
    })
  })

  it("can delete a page for a Document", () => {
    cy.api(routes.DELETE_PAGE(DELETE_PAGE_TARGET_URN, DELETE_PAGE_CASE_ID, DELETE_PAGE_DOCUMENT_ID, DELETE_PAGE_VERSION_ID, "PHASE_1"))
      .then(({ body }) => {
        console.log(body);
      })
  })
})
