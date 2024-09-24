import { CASE_SEARCH_ROUTE } from "../../src/mock-api/routes";

describe("Cms Authentication", () => {
  it("can throw if api returns 403 after Polaris has tried to reauthenticate", () => {
    cy.overrideRoute(CASE_SEARCH_ROUTE, {
      type: "break",
      httpStatusCode: 401,
    });

    cy.visit("/case-search-results?urn=12AB1111111&auth-refresh");
    cy.contains("CMS_AUTH_ERROR");
  });
});

export {};
