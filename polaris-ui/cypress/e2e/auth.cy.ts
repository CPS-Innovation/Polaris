import { CASE_SEARCH_ROUTE } from "../../src/mock-api/routes";

describe("Cms Authentication", () => {
  it("can reauthenticate if api returns 403", () => {
    cy.overrideRoute(CASE_SEARCH_ROUTE, {
      type: "break",
      httpStatusCode: 403,
    });

    cy.visit("/case-search-results?urn=12AB1111111");
    cy.location().should((location) => {
      expect(location.href).to.eq(
        "http://127.0.0.1:3000/case-search-results?polaris-ui-url=http%3A%2F%2F127.0.0.1%3A3000%2Fcase-search-results%3Furn%3D12AB1111111%26auth-refresh"
      );
    });
  });

  it("can throw if api returns 403 after Polaris has tried to reauthenticate", () => {
    cy.overrideRoute(CASE_SEARCH_ROUTE, {
      type: "break",
      httpStatusCode: 403,
    });

    cy.visit("/case-search-results?urn=12AB1111111&auth-refresh");
    cy.contains("CMS_AUTH_ERROR");
  });
});

export {};
