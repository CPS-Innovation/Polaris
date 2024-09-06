import { CASE_SEARCH_ROUTE } from "../../src/mock-api/routes";

describe("Cms Authentication", () => {
  it("can reauthenticate if api returns 403", () => {
    cy.overrideRoute(CASE_SEARCH_ROUTE, {
      type: "break",
      httpStatusCode: 401,
    });

    cy.visit("/case-search-results?urn=12AB1111111");
    cy.location().should((location) => {
      expect(location.href).to.contains(
        "http://127.0.0.1:3000/case-search-results?r=%2Fanother%2Fpath%3Fpolaris-ui-url%3Dhttp%253A%252F%252F127.0.0.1%253A3000%252Fcase-search-results%253Furn%253D12AB1111111%2526auth-refresh%26fail-correlation-id%3D"
      );
    });
  });

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
