import { CASE_SEARCH_ROUTE } from "../../src/mock-api/routes";

describe("authentication against CMS", () => {
  Cypress.on("uncaught:exception", (err, runnable) => {
    // returning false here prevents Cypress from
    // failing the test
    return false;
  });

  it("when the gateway API returns a 403 response it will atempt to reobtain CMS cookies", () => {
    cy.visit("/case-search");
    cy.overrideRoute(CASE_SEARCH_ROUTE, {
      type: "break",
      httpStatusCode: 403,
    });

    cy.visit("/case-search-results?urn=12AB1111111");

    cy.findByTestId("txt-please-wait-page-heading").should("exist");
    cy.findByTestId("txt-error-page-heading").should("not.exist");

    // cy.location("href").should(
    //   "eq",
    //   ""https://not-a-real-domain.local/polaris?q=http%3A%2F%2F127.0.0.1%3A3000%2Fcase-search-results%3Furn%3D12AB1111111%26auth-refresh""
    // );
    // cy.findByTestId("txt-error-page-heading").should("not.exist");
  });
});

export {};
