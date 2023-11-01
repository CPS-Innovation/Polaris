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
    it("Should show the feedback from link", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("feedback-banner")
        .should("exist")
        .contains(
          "Your feedback (opens in a new tab) will help us to improve this service."
        );
    });
    it("For Single defendant and single charge, should show defendant details, charge details and custody time limits and Youth Offender if applicable", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("defendant-details").then(($details) => {
        cy.wrap($details).contains("Walsh, Steve");
        cy.wrap($details).contains("DOB: 28 Nov 1977. Age: 45");
        cy.wrap($details).contains("Youth Offender");
      });

      cy.findByTestId("div-charges").then(($charges) => {
        cy.wrap($charges).findByTestId("charges-title").contains("Charges");
        cy.wrap($charges).contains("Custody time limit: 20 Days");
        cy.wrap($charges).contains("Custody end: 20 Nov 2022");
      });
    });

    it("For Single defendant and single charge,should read name from organisationName and shouldn't show date of birth in defendant details, if the defendant is an organisation ", () => {
      cy.visit("/case-search-results?urn=12AB1111122");
      cy.visit("/case-details/12AB1111122/13501");
      cy.findByTestId("txt-case-urn").contains("12AB1111122");
      cy.findByTestId("defendant-details").then(($details) => {
        cy.wrap($details)
          .findByTestId("txt-defendant-name")
          .should("have.text", "GUZZLERS BREWERY");
        cy.wrap($details).findByTestId("txt-defendant-DOB").should("not.exist");
      });
    });

    it("For multiple defendants, should show list of defendant names in the ascending order of listOrder and shouldn't show charge details", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13301");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "Walsh, Steve")
        .next()
        .should("have.text", "Taylor, Scott")
        .next()
        .should("have.text", "Victor, Peter");
      cy.findByTestId("link-defendant-details").contains(
        "View 3 defendants and charges"
      );
    });

    it("For multiple defendant, should read name from organisationName, if the defendant is an organisation", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13601");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "GUZZLERS BREWERY")
        .next()
        .should("have.text", "Victor, Peter");
      cy.findByTestId("link-defendant-details").contains(
        "View 2 defendants and charges"
      );
    });

    it("For multiple charges, should show list of defendant name and shouldn't show charge details", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13201");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "Walsh, Steve");
      cy.findByTestId("link-defendant-details").contains(
        "View 1 defendant and charges"
      );
    });

    it("For multiple charges / defendants, it can open the defendant and charges pdf and user should not be able to redact that document", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13201");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("list-defendant-names")
        .get("li")
        .first()
        .should("have.text", "Walsh, Steve");
      cy.findByTestId("link-defendant-details").contains(
        "View 1 defendant and charges"
      );
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("CASE OUTLINE");
      cy.findByTestId("tab-active").contains("CM01");
      cy.findByTestId("link-defendant-details").click();
      cy.findByTestId("tab-active").contains("Test DAC");
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("CASE OUTLINE");
      cy.wait(500);
      cy.selectPDFTextElement("This is a DV case.");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-warning").should("have.length", 1);
      cy.findByTestId("redaction-warning").contains(
        "Redaction is not supported for this document type."
      );
    });
  });

  // describe("pdf viewing", () => {
  //   it("can open a pdf", () => {
  //     cy.visit("/case-search-results?urn=12AB1111111");
  //     cy.visit("/case-details/12AB1111111/13401");
  //     cy.findByTestId("btn-accordion-open-close-all").click();

  //     cy.findByTestId("div-pdfviewer-0").should("not.exist");

  //     cy.findByTestId("link-document-1").click();

  //     cy.findByTestId("div-pdfviewer-0")
  //       .should("exist")
  //       .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
  //   });

  //   it("can open a pdf in a new tab", () => {
  //     cy.visit("/case-details/12AB1111111/13401", {
  //       onBeforeLoad(window) {
  //         cy.stub(window, "open");
  //       },
  //     });

  //     cy.findByTestId("btn-accordion-open-close-all").click();

  //     cy.findByTestId("link-document-1").click();

  //     cy.findByTestId("btn-open-pdf").click();

  //     cy.window()
  //       .its("open")
  //       .should(
  //         "be.calledWith",
  //         "https://mocked-out-api/api/some-complicated-sas-url/MCLOVEMG3",
  //         "_blank"
  //       );
  //   });
  // });

  describe("Document navigation away alert modal", () => {
    it("Should show an alert modal when closing a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
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
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
    });

    it("Should not show an alert modal when closing a document when there are no active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
    });
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

    it("Should show browser confirm modal when navigating away by refreshing the page, from a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.reload();
      cy.get("@beforeunloadCallback").should("have.been.calledOnce");
    });

    it("Should not show browser confirm modal when navigating away by refreshing the page, from a document with no active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.reload();
      cy.get("@beforeunloadCallback").should("not.have.been.called");
    });

    it("Should show browser confirm modal when navigating away using Header Crown Prosecution Service link, from a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("link-homepage").click();
      cy.get("@beforeunloadCallback").should("have.been.calledOnce");
    });

    it("Should show custom alert modal when navigating away using 'find a case' link from a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("link-back-link").click();
      cy.get("@beforeunloadCallback").should("not.have.been.called");
      cy.findByTestId("div-modal").then(($modal) => {
        cy.wrap($modal).contains("You have 1 document with unsaved redactions");
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
    });

    it("Should show custom alert modal when navigating away using browser back button from a document with active redactions", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.findByTestId("link-12AB1111111").click();
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
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
    });
  });

  describe("Document Presentation Flags", () => {
    const openAndRedactDocument = (linkId: string) => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId(linkId).click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("Not Disclosable");
      cy.selectPDFTextElement("Not Disclosable");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-warning").should("have.length", 1);
    };
    it("Redaction shouldn't be allowed and User should show warning message when selecting a text, if presentationFlags write status is 'DocTypeNotAllowed'", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("CASE OUTLINE");
      cy.selectPDFTextElement("This is a DV case.");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-warning").should("have.length", 1);
      cy.findByTestId("redaction-warning").contains(
        "Redaction is not supported for this document type."
      );
    });

    it("Redaction shouldn't be allowed and User should show warning message when selecting a text,if presentationFlags write status is `OnlyAvailableInCms`", () => {
      openAndRedactDocument("link-document-8");
      cy.findByTestId("redaction-warning").contains(
        "This document can only be redacted in CMS."
      );
    });
    it("Redaction shouldn't be allowed and User should show warning message when selecting a text,if presentationFlags write status is `OriginalFileTypeNotAllowed`", () => {
      openAndRedactDocument("link-document-5");
      cy.findByTestId("redaction-warning").contains(
        "Redaction is not supported for this file type."
      );
    });

    it("Redaction shouldn't be allowed and User should show warning message when selecting a text,if presentationFlags write status is `IsNotOcrProcessed`", () => {
      openAndRedactDocument("link-document-7");
      cy.findByTestId("redaction-warning").contains(
        "Awaiting OCR processing in CMS. Please try again later for redaction."
      );
    });

    it("User shouldn't be allowed to view document and there should be document view warnings, if presentationFlags read status is not 'Ok'", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-3").should("have.length", 0);
      cy.findByTestId("view-warning-court-preparation").should(
        "have.length",
        1
      );
      cy.findByTestId("view-warning-court-preparation").contains(
        "Some documents for this case are only available in CMS"
      );
      cy.findByTestId("name-text-document-3").should("have.length", 1);
      cy.findByTestId("name-text-document-3").contains("Doc_3");
      cy.findByTestId("view-warning-document-3").should("have.length", 1);
      cy.findByTestId("view-warning-document-3").contains(
        "Document only available on CMS"
      );
    });
  });

  describe("Report an Issue", () => {
    it("Should show the `Report an issue` button with correct text and show the confirmation modal when user has reported an issue", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("div-pdfviewer-0").should("not.exist");

      cy.findByTestId("link-document-1").click();

      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("btn-report-issue-0").should("exist");
      cy.findByTestId("btn-report-issue-0").contains("Report an issue");
      cy.findByTestId("btn-report-issue-0").click();
      cy.findByTestId("btn-report-issue-0").contains("Issue reported");
      cy.findByTestId("btn-report-issue-0").should("be.disabled");
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("Thanks for reporting an issue with this document.");
      cy.findByTestId("btn-modal-close").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("btn-report-issue-0").contains("Issue reported");
      cy.findByTestId("btn-report-issue-0").should("be.disabled");
    });
  });

  describe("Unsaved redactions accessibility through keyboard", () => {
    const verifyAriaDescriptionTextContent = (textContent: string) => {
      cy.focused().then((button) => {
        const siblingP = button.next("p");
        cy.wrap(siblingP).should("contain.text", textContent);
      });
    };

    it("Should be able to tab forward through each of the unsaved redactions in multiple pages", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.selectPDFTextElement("NORTH MARSH");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-report-issue-0").focus();

      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("PC Blaynee");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("EOIN MCLOVE");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Approved for referral to CPS:");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Instructions to Court Prosecutor:");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("POCA case");
      cy.findByTestId("btn-redact").click();

      cy.get("#btn-report-issue-0").focus();
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress("Tab");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("NORTH MARSH");
      cy.realPress("Tab");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("EOIN MCLOVE");
      cy.realPress("Tab");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("PC Blaynee");
      cy.realPress("Tab");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("Approved for referral to CPS:");
      cy.realPress("Tab");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("Instructions to Court Prosecutor:");
      cy.realPress("Tab");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("POCA case");
      cy.realPress("Tab");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-link-removeAll-0");
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    it("Should be able to tab + shift backward through each of the unsaved redactions added in different order but sorted by top left - bottom right", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.selectPDFTextElement("NORTH MARSH");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("EOIN MCLOVE");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Approved for referral to CPS:");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("PC Blaynee");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Instructions to Court Prosecutor:");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("POCA case");
      cy.findByTestId("btn-redact").click();

      cy.get("#btn-link-removeAll-0").focus();
      cy.realPress("Tab");
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);

      verifyAriaDescriptionTextContent("POCA case");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("Instructions to Court Prosecutor:");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("Approved for referral to CPS:");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("PC Blaynee");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("EOIN MCLOVE");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("NORTH MARSH");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");

      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "btn-report-issue-0");
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    it("Should be able to tab through each of the unsaved redactions added in different order but sorted by top left - bottom right", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.selectPDFTextElement("NORTH MARSH");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Dangerous offender:");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Date of birth:");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Police incident log:");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("PC JONES");
      cy.findByTestId("btn-redact").click();

      cy.get("#btn-report-issue-0").focus();
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("Date of birth:");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("Dangerous offender:");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("Police incident log:");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("PC JONES");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("NORTH MARSH");
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    it("Should be able to tab forward and backward skipping the `Report an issue` btn, if it is disabled ", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("btn-report-issue-0").click();
      cy.findByTestId("btn-modal-close").click();
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("tab-remove").focus();
      cy.realPress(["Tab"]);
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "btn-link-removeAll-0");
      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "active-tab-panel");
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    it("When tabbing from an unsaved redaction button, it should move the focus to remove redaction button and (shift +tab ) from remove redaction button should focus corresponding unsaved redaction button", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").click();
      cy.get("#btn-report-issue-0").focus();
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "btn-report-issue-0");
      cy.findByTestId("btn-link-removeAll-0").click();
    });
  });

  describe("Document texts accessibility through keyboard", () => {
    const keyPressAndVerifySelection = (
      direction: "forward" | "backward",
      text: string
    ) => {
      if (direction === "forward") {
        cy.realPress(["Control", ","]);
      }
      if (direction === "backward") {
        cy.realPress(["Alt", "Control", ","]);
      }

      cy.window().then((win) => {
        const selection = win.document.getSelection();
        expect(selection?.toString()).to.contain(text);
      });
    };
    it("Should be able to tab forward and backward through span elements in a document page using key ',' and 'Shift'+','", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      keyPressAndVerifySelection("forward", "W");
      keyPressAndVerifySelection("forward", "Y");
      keyPressAndVerifySelection("forward", "P");
      keyPressAndVerifySelection("forward", "R");
      keyPressAndVerifySelection("forward", "(");
      keyPressAndVerifySelection("backward", "R");
      keyPressAndVerifySelection("backward", "P");
      keyPressAndVerifySelection("backward", "Y");
      keyPressAndVerifySelection("backward", "W");
      keyPressAndVerifySelection("backward", "W");
    });

    it("Should be able to select and redact using keyboard", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("WEST YORKSHIRE POLICE");
      keyPressAndVerifySelection("forward", "W");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-redact");
      cy.realPress("Enter");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-count-text").contains("There is 1 redaction");
      cy.findByTestId("btn-save-redaction-0").should("exist");
      keyPressAndVerifySelection("forward", "Y");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "btn-redact");
      cy.realPress("Enter");
      cy.findByTestId("redaction-count-text").contains(
        "There are 2 redactions"
      );
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    it("Should lock the focus on the redact btn if the btn is present, when pressing both 'shift+tab' and 'tab' and release if the redact btn is not present", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-4").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("CASE FILE EVIDENCE and INFORMATION ");
      cy.wait(500);
      cy.realPress(["Control", ","]);
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-redact");
      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "btn-redact");
      cy.realPress("Escape");
      cy.focused().should("have.id", "active-tab-panel");
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "btn-report-issue-0");
    });

    it("Should be able to tab forward and backward through span elements in multiple document tabs pages using key ',' and 'Shift'+','", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      keyPressAndVerifySelection("forward", "W");
      cy.realPress(["Control", ","]);
      keyPressAndVerifySelection("forward", "P");
      keyPressAndVerifySelection("forward", "R");
      keyPressAndVerifySelection("backward", "P");
      //open the next document
      cy.findByTestId("link-document-4").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("CASE FILE EVIDENCE and INFORMATION");
      cy.wait(500);
      cy.realPress(["Control", ","]);
      cy.realPress(["Control", ","]);
      cy.realPress(["Control", ","]);
      cy.realPress(["Control", ","]);
      keyPressAndVerifySelection("forward", "R");
      keyPressAndVerifySelection("forward", "w");
      keyPressAndVerifySelection("forward", "c");
      keyPressAndVerifySelection("forward", "M");
      keyPressAndVerifySelection("forward", "6");
      keyPressAndVerifySelection("backward", "M");
      keyPressAndVerifySelection("backward", "c");
      keyPressAndVerifySelection("backward", "w");
      keyPressAndVerifySelection("backward", "R");
      //switch back to the first document
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      keyPressAndVerifySelection("forward", "W");
      cy.realPress(["Control", ","]);
      keyPressAndVerifySelection("forward", "P");
      keyPressAndVerifySelection("forward", "R");
      keyPressAndVerifySelection("backward", "P");
    });
  });

  describe("Switch main content areas using the Period '.' key press", () => {
    it("Should be able switch between main content areas using the Period '.' Key Press", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "side-panel");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "document-tabs");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "active-tab-panel");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "side-panel");
    });

    it("Should continue from the last active content if the focus has been changed to the inner element", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "side-panel");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "document-tabs");
      cy.findAllByTestId("btn-tab-0").click();
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "document-tabs");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "active-tab-panel");
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-report-issue-0");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "active-tab-panel");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "side-panel");
    });

    it("Should keep the focus on side-panel, if there are no documents open  while pressing the Period '.' Key", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "side-panel");
      cy.realPress(["Control", "."]);
      cy.focused().should("have.id", "side-panel");
    });
  });

  describe("Document Tabs", () => {
    it("The previous and next tab btn should be disabled,when there no more tabs to go on their side ", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("btn-tab-previous").should("be.disabled");
      cy.findByTestId("btn-tab-next").should("be.disabled");
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_0");
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_1");
      cy.findByTestId("btn-tab-previous").should("not.be.disabled");
      cy.findByTestId("btn-tab-next").should("be.disabled");
      cy.findByTestId("btn-tab-previous").click();
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_0");
      cy.findByTestId("btn-tab-previous").should("be.disabled");
      cy.findByTestId("btn-tab-next").should("not.be.disabled");
    });

    it("Should disable the tabsDropdown button, if there is only one tab opened and enable if more than one tab is opened", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("tabs-dropdown").should("be.disabled");
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("tabs-dropdown").should("not.be.disabled");
    });

    it("Should open and close the dropdown panel, when the  dropdown button is clicked ", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("tabs-dropdown").should("be.disabled");
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("tabs-dropdown").should("not.be.disabled");
      cy.findByTestId("tabs-dropdown-panel").should("not.exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("tabs-dropdown-panel").should("exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("tabs-dropdown-panel").should("not.exist");
    });

    it("Should be able make a tab active, by clicking on the open document link buttons from the dropdown panel and link button for current active tab should be disabled", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_1");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("tabs-dropdown-panel").should("exist");
      cy.findByTestId("tabs-dropdown-panel")
        .contains("MCLOVEMG3")
        .should("not.be.disabled");
      cy.findByTestId("tabs-dropdown-panel")
        .contains("CM01")
        .should("be.disabled");
      cy.findByTestId("tabs-dropdown-panel").contains("MCLOVEMG3").click();
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_0");
      cy.findByTestId("tabs-dropdown-panel").should("not.exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("tabs-dropdown-panel")
        .contains("MCLOVEMG3")
        .should("be.disabled");
      cy.findByTestId("tabs-dropdown-panel")
        .contains("CM01")
        .should("not.be.disabled");
    });

    it("Should be able close the dropdown panel when you press 'escape' key  or click outside of the panel", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_1");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("tabs-dropdown-panel").should("exist");
      cy.findByTestId("tabs-dropdown-panel")
        .contains("MCLOVEMG3")
        .should("not.be.disabled");
      cy.findByTestId("tabs-dropdown-panel")
        .contains("CM01")
        .should("be.disabled");
      cy.realPress("Escape");
      cy.findByTestId("tabs-dropdown-panel").should("not.exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("tabs-dropdown-panel").should("exist");
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("tabs-dropdown-panel").should("not.exist");
    });
  });
});

export {};
