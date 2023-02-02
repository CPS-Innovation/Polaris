import { CASE_ROUTE } from "../../../src/mock-api/routes";

describe("case details page", () => {
  describe("case page navigation", () => {
    it("can navigate back from case page, having not previously visited results page, and land on search page", () => {
      cy.visit("/case-details/12AB1111111/13401");

      cy.findAllByTestId("link-back-link").should("have.attr", "href", "/");

      cy.findAllByTestId("link-back-link").click();
      cy.location("pathname").should("eq", "/case-search");
    });

    it("can navigate back from case page, having previously visited results page, and land on results page", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.findByTestId("link-12AB1111111").click();

      cy.findAllByTestId("link-back-link").should(
        "have.attr",
        "href",
        "/case-search-results?urn=12AB1111111"
      );

      cy.findAllByTestId("link-back-link").click();

      cy.location("pathname").should("eq", "/case-search-results");
      cy.location("search").should("eq", "?urn=12AB1111111");
    });

    it("shows the unhandled error page if an unexpected error occurrs with the api", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.overrideRoute(CASE_ROUTE, {
        type: "break",
        httpStatusCode: 500,
      });

      cy.findByTestId("link-12AB1111111").click();

      // we are showing the error page
      cy.findByTestId("txt-error-page-heading");
    });
  });

  describe("case details", () => {
    it("can show case details", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("txt-defendant-name").contains("Walsh, Steve");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
    });
  });

  describe("pdf viewing", () => {
    it("can open a pdf", { defaultCommandTimeout: 15000 }, () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("div-pdfviewer").should("not.exist");

      cy.findByTestId("link-document-1").click();

      cy.findByTestId("div-pdfviewer")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    });

    it("can open a pdf in a new tab", () => {
      cy.visit("/case-details/12AB1111111/13401", {
        onBeforeLoad(window) {
          cy.stub(window, "open");
        },
      });

      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("link-document-1").click();

      cy.findByTestId("btn-open-pdf").click();

      cy.window()
        .its("open")
        .should(
          "be.calledWith",
          "https://mocked-out-api/api/some-complicated-sas-url/MCLOVEMG3",
          "_blank"
        );
    });
  });

  describe("Document navigation away alert modal", () => {
    it(
      "Should show an alert modal when navigating away from a document with active redactions",
      { defaultCommandTimeout: 15000 },
      () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("btn-accordion-open-close-all").click();
        cy.findByTestId("link-document-1").click();
        cy.findByTestId("div-pdfviewer")
          .should("exist")
          .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
        cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
        cy.findByTestId("btn-redact").should("have.length", 1);
        cy.findByTestId("btn-redact").click({ force: true });
        cy.findAllByTestId("tab-remove").click();
        cy.findAllByTestId("div-modal")
          .should("exist")
          .contains("You have unsaved redactions");
        // click on return to case file btn
        cy.findAllByTestId("btn-nav-return").click();
        cy.findAllByTestId("div-modal").should("not.exist");
        cy.findAllByTestId("tab-remove").click();
        cy.findAllByTestId("div-modal")
          .should("exist")
          .contains("You have unsaved redactions");
        // click on ignore btn
        cy.findAllByTestId("btn-nav-ignore").click();
        cy.findAllByTestId("div-modal").should("not.exist");
        cy.findByTestId("div-pdfviewer").should("not.exist");
      }
    );

    it(
      "Should not show an alert modal when navigating away from a document when there are no active redactions",
      { defaultCommandTimeout: 15000 },
      () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("btn-accordion-open-close-all").click();
        cy.findByTestId("link-document-1").click();
        cy.findByTestId("div-pdfviewer")
          .should("exist")
          .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
        cy.findAllByTestId("tab-remove").click();
        cy.findAllByTestId("div-modal").should("not.exist");
        cy.findByTestId("div-pdfviewer").should("not.exist");
      }
    );
  });
});

export {};
