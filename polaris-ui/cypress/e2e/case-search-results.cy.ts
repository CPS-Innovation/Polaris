import { CASE_SEARCH_ROUTE } from "../../src/mock-api/routes";

describe("search results", () => {
  it("displays search result and summarises returned count and URN", () => {
    cy.visit("/case-search-results?urn=12AB1111111");

    cy.findByTestId("link-12AB1111111", { timeout: 20000 });
    cy.findByTestId("txt-result-count").contains("1");
    cy.findByTestId("txt-result-urn").contains("12AB1111111");

    cy.visit("/case-search-results?urn=12AB2222222");

    cy.findByTestId("link-12AB2222222/1");
    cy.findByTestId("link-12AB2222222/2");
    cy.findByTestId("txt-result-count").contains("2");
    cy.findByTestId("txt-result-urn").contains("12AB2222222");
  });

  it("can navigate to a split URL case using the root URN in the URL and not the split URN", () => {
    cy.visit("/case-search-results?urn=12AB2222222");
    cy.findByTestId("link-12AB2222222/1").click();
    cy.url().should("include", "/case-details/12AB2222222/13401");
  });

  it("can not accept an invalid URN and return an appropriate validation message to the user", () => {
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

  it("shows the unhandled error page if an unexpected error occurrs with the api", () => {
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

  it("does not show error if gateway call returns 404", () => {
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

  it("Shouldn't show the  surname,firstname and date of birth and read name from organisationName, if defendant is an organization", () => {
    cy.visit("/case-search-results?urn=12AB2222233");

    cy.findAllByTestId("link-12AB2222233", { timeout: 20000 });
    cy.findByTestId("txt-result-count").contains("1");
    cy.findByTestId("defendant-name-text-0").should(
      "have.text",
      "GUZZLERS BREWERY and others"
    );
    cy.findByTestId("defendant-DOB-0").should("not.exist");
  });

  it("Should show the first name and surname and date of birth if defendant is not an organization", () => {
    cy.visit("/case-search-results?urn=12AB1111111");

    cy.findByTestId("link-12AB1111111", { timeout: 20000 });
    cy.findByTestId("txt-result-count").contains("1");
    cy.findByTestId("defendant-name-text-0").should("have.text", "Walsh,Steve");
    cy.findByTestId("defendant-DOB-0").should(
      "have.text",
      "Date of birth: 28 November 1977"
    );
  });
});

export {};
