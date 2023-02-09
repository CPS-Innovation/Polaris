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
      "Should show an alert modal when closing a document with active redactions",
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
        cy.findByTestId("tab-remove").click();
        cy.findByTestId("div-modal")
          .should("exist")
          .contains("You have unsaved redactions");
        // click on return to case file btn
        cy.findByTestId("btn-nav-return").click();
        cy.findByTestId("div-modal").should("not.exist");
        cy.findByTestId("tab-remove").click();
        cy.findByTestId("div-modal")
          .should("exist")
          .contains("You have unsaved redactions");
        // click on ignore btn
        cy.findByTestId("btn-nav-ignore").click();
        cy.findByTestId("div-modal").should("not.exist");
        cy.findByTestId("div-pdfviewer").should("not.exist");
      }
    );

    it(
      "Should not show an alert modal when closing a document when there are no active redactions",
      { defaultCommandTimeout: 15000 },
      () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("btn-accordion-open-close-all").click();
        cy.findByTestId("link-document-1").click();
        cy.findByTestId("div-pdfviewer")
          .should("exist")
          .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
        cy.findByTestId("tab-remove").click();
        cy.findByTestId("div-modal").should("not.exist");
        cy.findByTestId("div-pdfviewer").should("not.exist");
      }
    );
  });

  describe("Navigating away from case file", () => {
    /* 
    We are mocking the onbeforeunload event callback function, to avoid triggering the alert in the cypress test,
    this will Prevent Test Runner hanging when the application uses confirmation dialog in window.onbeforeunload callback.
    Adding of stub callback to the beforeunload event is triggered by the logic in the application.
    This way we make sure teh call back is been fired.
    */
    beforeEach(() => {
      const beforeunloadCallbackStub = cy.stub().as("beforeunloadCallback");
      cy.on("window:before:load", (win) => {
        Object.defineProperty(win, "onbeforeunload", {
          set(cb) {
            if (cb !== null) {
              win.addEventListener("beforeunload", beforeunloadCallbackStub);
            }
          },
        });
      });
    });

    it(
      "Should show browser confirm modal when navigating away by refreshing the page, from a document with active redactions",
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
        cy.findByTestId("btn-redact").click();
        cy.reload();
        cy.get("@beforeunloadCallback").should("have.been.calledOnce");
      }
    );

    it(
      "Should not show browser confirm modal when navigating away by refreshing the page, from a document with no active redactions",
      { defaultCommandTimeout: 15000 },
      () => {
        cy.visit("/case-details/12AB1111111/13401");
        cy.findByTestId("btn-accordion-open-close-all").click();
        cy.findByTestId("link-document-1").click();
        cy.findByTestId("div-pdfviewer")
          .should("exist")
          .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
        cy.reload();
        cy.get("@beforeunloadCallback").should("not.have.been.called");
      }
    );

    it(
      "Should show browser confirm modal when navigating away using Header Crown Prosecution Service link, from a document with active redactions",
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
        cy.findByTestId("btn-redact").click();
        cy.findByTestId("link-homepage").click();
        cy.get("@beforeunloadCallback").should("have.been.calledOnce");
      }
    );

    it(
      "Should show custom alert modal when navigating away using 'find a case' link from a document with active redactions",
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
        cy.findByTestId("btn-redact").click();
        cy.findByTestId("link-back-link").click();
        cy.get("@beforeunloadCallback").should("not.have.been.called");
        cy.findByTestId("div-modal").then(($modal) => {
          cy.wrap($modal).contains(
            "You have 1 document with unsaved redactions"
          );
          //checks for the pdf link in the modal and clicking on the link
          cy.wrap($modal)
            .findByTestId("link-document-1")
            .contains("MCLOVEMG3")
            .click();
        });
        cy.findByTestId("div-modal").should("not.exist");
        cy.findByTestId("link-back-link").click();
        cy.findByTestId("div-modal")
          .should("exist")
          .contains("You have 1 document with unsaved redactions");
        cy.findByTestId("btn-nav-ignore").click();
        cy.findByTestId("div-modal").should("not.exist");
        cy.location("pathname").should("eq", "/case-search");
      }
    );

    it(
      "Should show custom alert modal when navigating away using browser back button from a document with active redactions",
      { defaultCommandTimeout: 15000 },
      () => {
        cy.visit("/case-search-results?urn=12AB1111111");
        cy.findByTestId("link-12AB1111111").click();
        cy.findByTestId("btn-accordion-open-close-all").click();
        cy.findByTestId("link-document-1").click();
        cy.findByTestId("div-pdfviewer")
          .should("exist")
          .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
        cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
        cy.findByTestId("btn-redact").should("have.length", 1);
        cy.findByTestId("btn-redact").click();
        cy.go(-1);
        //two times back button to reach the search page , first one was for the hash urls change with the pdf safeid
        cy.go(-1);
        cy.findByTestId("div-modal")
          .should("exist")
          .contains("You have 1 document with unsaved redactions");
        cy.findByTestId("btn-nav-return").click();
        cy.findByTestId("div-modal").should("not.exist");
        cy.go(-1);
        cy.findByTestId("div-modal")
          .should("exist")
          .contains("You have 1 document with unsaved redactions");
        cy.findByTestId("btn-nav-ignore").click();
        cy.findByTestId("div-modal").should("not.exist");
        cy.location("pathname").should("eq", "/case-search-results");
      }
    );
  });
});

export {};
