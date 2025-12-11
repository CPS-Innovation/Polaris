import { CASE_SEARCH_ROUTE } from "../../src/mock-api/routes";

describe("search results", () => {
  xit("displays search result and summarises returned count and URN", () => {
    cy.visit("/case-search-results?urn=45GD9800103");

    // cy.findByTestId("link-45GD9800103", { timeout: 40000 });
    cy.findByTestId("txt-result-count").contains("1");
    cy.findByTestId("txt-result-urn").contains("45GD9800103");

    cy.visit("/case-search-results?urn=12AB2222222");

    cy.findByTestId("link-45GD9800103/1");
    cy.findByTestId("link-45GD9800103/2");
    cy.findByTestId("txt-result-count").contains("2");
    cy.findByTestId("txt-result-urn").contains("45GD9800103");
  });

  // it("displays search result and summarises returned count and URN", () => {
  //   const urn1 = "45GD9800103";

  //   // Intercept the real backend call discovered in DevTools
  //   // Exact match for urn1:
  //   cy.intercept("GET", "http://localhost:7075/api/urns/45GD9800103/cases").as(
  //     "searchCases1"
  //   );

  //   // If your app is served under /polaris-ui in the browser, include that prefix here too.
  //   cy.visit(`/polaris-ui/case-search-results?urn=${urn1}`);

  //   // Wait for the data that populates the results
  //   cy.wait("@searchCases1");

  //   // Now the link should be present
  //   cy.findByTestId(`link-${urn1}`, { timeout: 20000 }).should("be.visible");
  //   cy.findByTestId("txt-result-count").contains("1");
  //   cy.findByTestId("txt-result-urn").contains(urn1);
  // });

  xit("can navigate to a split URL case using the root URN in the URL and not the split URN", () => {
    cy.visit("/case-search-results?urn=12AB2222222");
    cy.findByTestId("link-12AB2222222/1").click();
    cy.url().should("include", "/case-details/12AB2222222/13401");
  });

  xit("can not accept an invalid URN and return an appropriate validation message to the user", () => {
    cy.visit("/case-search-results?urn=12AB1111111");

    cy.findByTestId("input-search-urn-error").should("not.exist");
    cy.findByTestId("link-validation-urn").should("not.exist");

    cy.findByTestId("input-search-urn").type("XXX");
    cy.findByTestId("button-search").click();

    // validation summary and message show
    cy.findByTestId("input-search-urn").should("exist");

    cy.findByTestId("link-validation-urn")
      .should("exist")
      // should be able to link from validation message to input
      .focus()
      .click();

    cy.findByTestId("input-search-urn").should("have.focus");
  });

  xit("shows the unhandled error page if an unexpected error occurrs with the api", () => {
    cy.visit("/case-search");
    cy.overrideRoute(CASE_SEARCH_ROUTE, {
      type: "break",
      httpStatusCode: 599,
    });

    cy.findByTestId("input-search-urn").type("12AB1111111");
    cy.findByTestId("button-search").click();

    // we are showing the error page
    cy.findByTestId("txt-error-page-heading");
  });

  xit("does not show error if gateway call returns 404", () => {
    cy.visit("/case-search");
    cy.overrideRoute(CASE_SEARCH_ROUTE, {
      type: "break",
      httpStatusCode: 404,
    });

    cy.findByTestId("input-search-urn").type("12AB1111111");
    cy.findByTestId("button-search").click();

    // we have remianed on the results page
    cy.url().should("include", "/case-search-results");
  });

  xit("Shouldn't show the  surname,firstname and date of birth and read name from organisationName, if defendant is an organization", () => {
    cy.visit("/case-search-results?urn=12AB2222233");

    cy.findAllByTestId("link-12AB2222233", { timeout: 20000 });
    cy.findByTestId("txt-result-count").contains("1");
    cy.findByTestId("defendant-name-text-0").should(
      "have.text",
      "GUZZLERS BREWERY and others"
    );
    cy.findByTestId("defendant-DOB-0").should("not.exist");
  });

  xit("Should show the first name and surname and date of birth if defendant is not an organization", () => {
    cy.visit("/case-search-results?urn=12AB1111111");

    cy.findByTestId("link-12AB1111111", { timeout: 20000 });
    cy.findByTestId("txt-result-count").contains("1");
    // cy.findByTestId("defendant-name-text-0").should("have.text", "Walsh,Steve");
    cy.findByTestId("defendant-DOB-0").should(
      "have.text",
      "Date of birth: 28 November 1977"
    );
  });
});

export {};
