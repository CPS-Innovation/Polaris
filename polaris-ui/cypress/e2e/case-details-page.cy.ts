import { CASE_ROUTE, FILE_ROUTE } from "../../src/mock-api/routes";
import { parseISO, differenceInYears } from "date-fns";

export const getAgeFromIsoDate = (isoDateString: string) =>
  isoDateString && differenceInYears(new Date(), parseISO(isoDateString));

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

    it("shows the unhandled error page if an unexpected error occurs with the api", () => {
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
    it("For Single defendant and single charge, should show defendant details, charge details and custody time limits and Youth offender if applicable", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("key-details").then(($details) => {
        cy.wrap($details).contains("Walsh, Steve");
        cy.wrap($details).contains(
          `DOB: 28 Nov 1977, Age: ${getAgeFromIsoDate("1977-11-28")}`
        );
        cy.wrap($details).contains("Youth offender");
      });

      cy.findByTestId("div-charges").then(($charges) => {
        cy.wrap($charges).findByTestId("charges-title").contains("Charges");
        cy.wrap($charges).contains("Custody time limit: 20 Days");
        cy.wrap($charges).contains("Custody end: 20 Nov 2022");
      });
    });

    it("For null LeadDefendant, shouldn't show defendant details", () => {
      cy.visit("/case-search-results?urn=12AB2222244");
      cy.visit("/case-details/12AB2222244/13701");
      cy.findByTestId("txt-case-urn").contains("12AB2222244");
      cy.findByTestId("defendant-name").should("not.exist");
      cy.findByTestId("txt-defendant-DOB").should("not.exist");
    });

    it("For Single defendant and single charge,should read name from organisationName and shouldn't show date of birth in defendant details, if the defendant is an organisation ", () => {
      cy.visit("/case-search-results?urn=12AB1111122");
      cy.visit("/case-details/12AB1111122/13501");
      cy.findByTestId("txt-case-urn").contains("12AB1111122");
      cy.findByTestId("key-details").then(($details) => {
        cy.wrap($details)
          .findByTestId("defendant-name")
          .should("have.text", "GUZZLERS BREWERY");
        cy.wrap($details).findByTestId("txt-defendant-DOB").should("not.exist");
      });
    });

    it("For multiple defendants, should show list of defendant names in the ascending order of listOrder and shouldn't show charge details", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13301");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("defendant-name").should(
        "have.text",
        "Walsh, Steve; Taylor, Scott; Victor, Peter"
      );
      cy.findByTestId("link-defendant-details").contains(
        "View 3 defendants and charges"
      );
    });

    it("For multiple defendant, should read name from organisationName, if the defendant is an organisation", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13601");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("defendant-name").should(
        "have.text",
        "GUZZLERS BREWERY; Victor, Peter"
      );

      cy.findByTestId("link-defendant-details").contains(
        "View 2 defendants and charges"
      );
    });

    it("For multiple charges, should show list of defendant name and shouldn't show charge details", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13201");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("defendant-name").should("have.text", "Walsh, Steve");
      cy.findByTestId("link-defendant-details").contains(
        "View 1 defendant and charges"
      );
    });

    it("For multiple charges / defendants, it can open the defendant and charges pdf and user should not be able to redact that document", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13201");
      cy.findByTestId("txt-case-urn").contains("12AB1111111");
      cy.findByTestId("div-charges").should("not.exist");
      cy.findByTestId("defendant-name").should("have.text", "Walsh, Steve");
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
      cy.selectPDFTextElement("This is a DV case.", "div-pdfviewer-1");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-warning").should("have.length", 1);
      cy.findByTestId("redaction-warning").contains(
        "Redaction is not supported for this document type."
      );
    });
  });

  describe("Document navigation away alert modal", () => {
    it("Should show an alert modal when closing a document with active redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
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
      const doc1CheckInCounter = { count: 0 };
      cy.trackRequestCount(
        doc1CheckInCounter,
        "DELETE",
        "/api/urns/12AB1111111/cases/13401/documents/1/versions/1/checkout"
      );
      // click on ignore btn
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
      cy.waitUntil(() => {
        return doc1CheckInCounter.count;
      }).then(() => {
        expect(doc1CheckInCounter.count).to.equal(1);
      });
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
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
      const doc1CheckInCounter = { count: 0 };
      cy.trackRequestCount(
        doc1CheckInCounter,
        "DELETE",
        "/api/urns/12AB1111111/cases/13401/documents/1/versions/1/checkout"
      );
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.location("pathname").should("eq", "/case-search");
      cy.waitUntil(() => {
        return doc1CheckInCounter.count;
      }).then(() => {
        expect(doc1CheckInCounter.count).to.equal(1);
      });
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
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

    it("Redaction shouldn't be allowed and User should show warning message when selecting a text,if presentationFlags write status is `IsDispatched`", () => {
      openAndRedactDocument("link-document-9");
      cy.findByTestId("redaction-warning").contains(
        "This is a dispatched document."
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
      cy.findByTestId("document-actions-dropdown-0").should("exist");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("dropdown-panel").contains("Report an issue").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains(`Report a problem with: "MCLOVEMG3"`);
      cy.findByTestId("btn-report-issue-save").should("be.disabled");
      cy.findByTestId("btn-report-issue-close").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel").contains("Report an issue").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains(`Report a problem with: "MCLOVEMG3"`);
      cy.findByTestId("btn-report-issue-save").should("be.disabled");
      cy.findByTestId("report-issue-more-details").type("hello");
      cy.findByTestId("btn-report-issue-save").should("not.be.disabled");
      cy.findByTestId("btn-report-issue-save").click();

      cy.findByTestId("div-modal")
        .should("exist")
        .contains("Thanks for reporting an issue with this document.");
      cy.findByTestId("btn-feedback-modal-ok").click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Report an issue")
        .should("not.exist");
      cy.findByTestId("dropdown-panel")
        .contains("Issue reported")
        .should("be.disabled");
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Report an issue")
        .should("not.exist");
      cy.findByTestId("dropdown-panel")
        .contains("Issue reported")
        .should("be.disabled");
    });
    it("User should be able to open multiple documents and handle its own `Report an issue`", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("document-actions-dropdown-0").should("exist");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("dropdown-panel").contains("Report an issue").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains(`Report a problem with: "MCLOVEMG3"`);
      cy.findByTestId("btn-modal-close").click();

      cy.findByTestId("div-pdfviewer-1").should("not.exist");
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("document-actions-dropdown-1").should("exist");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-1").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("dropdown-panel").contains("Report an issue").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains(`Report a problem with: "PortraitLandscape"`);
      cy.findByTestId("btn-modal-close").click();
    });
    it("User should be able to open document actions dropdown when multiple documents are open", () => {
      cy.visit("/case-search-results?urn=12AB1111111");
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("document-actions-dropdown-0").should("exist");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("document-actions-dropdown-1").should("exist");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-1").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("document-actions-dropdown-2").should("exist");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-2").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("btn-tab-0").click();
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("btn-tab-2").click();
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("document-actions-dropdown-2").click();
      cy.findByTestId("dropdown-panel").should("exist");
    });
  });

  describe("Unsaved redactions accessibility through keyboard", () => {
    const verifyAriaDescriptionTextContent = (textContent: string) => {
      cy.focused().then((button) => {
        const siblingP = button.next("p");
        cy.wrap(siblingP).should("contain.text", textContent);
      });
    };

    //Todo: add the test case delete page button is available and same with rotation page button is available
    xit("Should be able to tab forward through each of the unsaved redactions in multiple pages", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.contains("button", "Hide Delete Page Options").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.selectPDFTextElement("NORTH MARSH");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("document-actions-dropdown-0").focus();

      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("PC Blaynee");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("EOIN MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Approved for referral to CPS:");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Instructions to Court Prosecutor:");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("POCA case");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.get("#document-actions-dropdown-0").focus();
      cy.wait(500);
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress("Enter");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Escape");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("NORTH MARSH");
      cy.realPress("Enter");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Escape");
      cy.realPress("Tab");
      cy.realPress("Tab");
      cy.realPress("Tab");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("EOIN MCLOVE");
      cy.realPress("Enter");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Escape");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("PC Blaynee");
      cy.realPress("Enter");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Escape");
      cy.realPress("Tab");
      cy.realPress("Tab");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("Approved for referral to CPS:");
      cy.realPress("Enter");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Escape");
      cy.realPress("Tab");
      cy.realPress("Tab");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("Instructions to Court Prosecutor:");
      cy.realPress("Enter");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Escape");
      cy.realPress("Tab");
      cy.realPress("Tab");
      cy.realPress("Tab");
      verifyAriaDescriptionTextContent("POCA case");
      cy.realPress("Enter");
      cy.focused().should("have.id", "remove-btn");
      cy.realPress("Escape");
      cy.realPress("Tab");
      cy.realPress("Tab");
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-link-removeAll-0");
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    xit("Should be able to tab + shift backward through each of the unsaved redactions added in different order but sorted by top left - bottom right", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.contains("button", "Hide Delete Page Options").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.selectPDFTextElement("NORTH MARSH");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("EOIN MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Approved for referral to CPS:");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("PC Blaynee");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Instructions to Court Prosecutor:");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("POCA case");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.get("#btn-link-removeAll-0").focus();
      cy.realPress("Tab");
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);

      verifyAriaDescriptionTextContent("POCA case");
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("Instructions to Court Prosecutor:");

      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("Approved for referral to CPS:");

      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("PC Blaynee");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("EOIN MCLOVE");

      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("NORTH MARSH");

      cy.realPress(["Shift", "Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");

      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "document-actions-dropdown-0");
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    xit("Should be able to tab through each of the unsaved redactions added in different order but sorted by top left - bottom right", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.contains("button", "Hide Delete Page Options").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.selectPDFTextElement("NORTH MARSH");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Dangerous offender:");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Date of birth:");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Police incident log:");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("PC JONES");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.get("#document-actions-dropdown-0").focus();
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("Date of birth:");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("Dangerous offender:");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("Police incident log:");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("PC JONES");
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("NORTH MARSH");
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    xit("When pressing enter from an unsaved redaction button,should show the remove redaction modal alert and pressing escape should bring the focus corresponding unsaved redaction button", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.contains("button", "Hide Delete Page Options").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.get("#document-actions-dropdown-0").focus();
      cy.realPress(["Tab"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Enter"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Escape"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Enter"]);
      cy.focused().should("have.id", "remove-btn");
      cy.realPress(["Escape"]);
      verifyAriaDescriptionTextContent("WEST YORKSHIRE POLICE");
      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "document-actions-dropdown-0");
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

    xit("Should be able to select and redact using keyboard", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("WEST YORKSHIRE POLICE");
      keyPressAndVerifySelection("forward", "W");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.realPress("Tab");
      cy.focused().should("have.id", "select-redaction-type");
      cy.findByTestId("select-redaction-type").select("2");
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-redact");
      cy.realPress("Enter");
      cy.findByTestId("btn-redact").should("have.length", 0);
      cy.findByTestId("redaction-count-text-0").contains(
        "There is 1 redaction"
      );
      cy.findByTestId("btn-save-redaction-0").should("exist");
      keyPressAndVerifySelection("forward", "Y");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "select-redaction-type");
      cy.findByTestId("select-redaction-type").select("2");
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-redact");
      cy.realPress("Enter");
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.findByTestId("btn-link-removeAll-0").click();
    });

    xit("Should lock the focus on the redact btn if the btn is present, when pressing both 'shift+tab' and 'tab' and release if the redact btn is not present", () => {
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
      cy.focused().should("have.id", "select-redaction-type");
      cy.findByTestId("select-redaction-type").select("2");
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-redact");
      cy.realPress(["Shift", "Tab"]);
      cy.focused().should("have.id", "select-redaction-type");
      cy.findByTestId("select-redaction-type").select("2");
      cy.realPress("Tab");
      cy.focused().should("have.id", "btn-redact");
      cy.realPress("Escape");
      cy.focused().should("have.id", "active-tab-panel");
      cy.realPress(["Tab"]);
      cy.realPress(["Tab"]);
      cy.focused().should("have.id", "document-actions-dropdown-0");
    });

    xit("Should be able to tab forward and backward through span elements in multiple document tabs pages using key ',' and 'Shift'+','", () => {
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
      cy.wait(500);
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
      cy.realPress("Tab");
      cy.focused().should("have.id", "document-actions-dropdown-0");
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
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("dropdown-panel").should("not.exist");
    });

    it("Should be able make a tab active, by clicking on the open document link buttons from the dropdown panel and link button for current active tab should be disabled", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_1");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("dropdown-panel")
        .contains("MCLOVEMG3")
        .should("not.be.disabled");
      cy.findByTestId("dropdown-panel").contains("CM01").should("be.disabled");
      cy.findByTestId("dropdown-panel").contains("MCLOVEMG3").click();
      cy.findByTestId("tab-active").should("have.attr", "id", "tab_0");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("dropdown-panel")
        .contains("MCLOVEMG3")
        .should("be.disabled");
      cy.findByTestId("dropdown-panel")
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
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("dropdown-panel")
        .contains("MCLOVEMG3")
        .should("not.be.disabled");
      cy.findByTestId("dropdown-panel").contains("CM01").should("be.disabled");
      cy.realPress("Escape");
      cy.findByTestId("dropdown-panel").should("not.exist");
      cy.findByTestId("tabs-dropdown").click();
      cy.findByTestId("dropdown-panel").should("exist");
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("dropdown-panel").should("not.exist");
    });
  });

  describe("Hte emails", () => {
    it("Should show communication sub categories", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.get("#side-panel").scrollTo("bottom");
      cy.get("h2").contains("Communications").should("be.visible");
      cy.get("h2").contains("Communications").click();
      cy.get("#side-panel").scrollTo("bottom");
      cy.get("h3").contains("Communication files").should("be.visible");
      cy.get("h3").contains("Emails").should("be.visible");
      cy.get("h2").contains("Communications").click();
      cy.get("h3").contains("Communication files").should("not.be.visible");
      cy.get("h3").contains("Emails").should("not.be.visible");
    });
    it("Should show number of attachments in the accordion, list attachment document name  in the document attachment head and clicking on it should open the corresponding documents", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.get("#side-panel").scrollTo("bottom");
      cy.get("h2").contains("Communications").should("be.visible");
      cy.get("h2").contains("Communications").click();
      cy.get("#side-panel").scrollTo("bottom");
      cy.get("h3").contains("Communication files").should("be.visible");
      cy.get("h3").contains("Emails").should("be.visible");
      cy.findByTestId("attachment-text-4").should("have.text", "2 attachments");
      cy.findByTestId("link-document-4").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("CASE FILE EVIDENCE and INFORMATION");
      cy.findByTestId("doc-attach-btn-1").should("have.text", "MCLOVEMG3,");
      cy.findByTestId("doc-attach-btn-2").should("have.text", "CM01");
      cy.findByTestId("doc-attach-btn-1").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("tab-active").should("contain", "MCLOVEMG3");
      cy.findByTestId("btn-tab-0").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("CASE FILE EVIDENCE and INFORMATION");
      cy.findByTestId("doc-attach-btn-2").click();
      cy.findByTestId("tab-active").should("contain", "CM01");
      cy.findByTestId("div-pdfviewer-2")
        .should("exist")
        .contains("CASE OUTLINE");
    });
  });

  describe("Document Full Screen", () => {
    it("Should show the 'Show Full Screen' button when at least one document is opened in a tab", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("full-screen-btn").should("not.exist");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("full-screen-btn").should("exist");
    });

    it("Should hide the 'Show Full Screen' button when no documents are opened in a tab", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("full-screen-btn").should("not.exist");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("full-screen-btn").should("exist");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("full-screen-btn").should("not.exist");
    });

    it("Should show and hide the document in full screen on clicking the full screen btn and should show correct tooltip", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("side-panel").should("exist");
      cy.findByTestId("full-screen-btn").trigger("mouseover");
      cy.findByTestId("tooltip").should("contain", "View full screen");
      cy.findByTestId("full-screen-btn").click();
      cy.findByTestId("side-panel").should("not.exist");
      cy.findByTestId("full-screen-btn").trigger("mouseover");
      cy.findByTestId("tooltip").should("contain", "Exit full screen");
      cy.findByTestId("full-screen-btn").click();
      cy.findByTestId("side-panel").should("exist");
    });

    it("Should not show the 'Show Full Screen' button if the full screen feature is turned off", () => {
      cy.visit("/case-details/12AB1111111/13401?fullScreen=false");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("side-panel").should("exist");
      cy.findByTestId("full-screen-btn").should("not.exist");
    });
    it("Should retain accordion state when user comes back from full screen mode", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("link-document-10").should("not.be.visible");
      cy.findByTestId("exhibits").click();
      cy.findByTestId("link-document-10")
        .should("be.visible")
        .and("have.text", "PortraitLandscape");
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("Page1 Portrait");
      cy.findByTestId("full-screen-btn").click();
      cy.findByTestId("side-panel").should("not.exist");
      cy.findByTestId("full-screen-btn").click();
      cy.findByTestId("side-panel").should("exist");
      cy.findByTestId("link-document-10")
        .should("be.visible")
        .and("have.text", "PortraitLandscape");
      cy.findByTestId("link-document-1").scrollIntoView();
      cy.findByTestId("link-document-1").should("not.be.visible");
      cy.findByTestId("communications").click();
      cy.findByTestId("link-document-1").scrollIntoView();
      cy.findByTestId("link-document-1")
        .should("be.visible")
        .and("have.text", "MCLOVEMG3");
      cy.findByTestId("full-screen-btn").click();
      cy.findByTestId("side-panel").should("not.exist");
      cy.findByTestId("full-screen-btn").click();
      cy.findByTestId("link-document-10")
        .should("be.visible")
        .and("have.text", "PortraitLandscape");
      cy.findByTestId("link-document-1").scrollIntoView();
      cy.findByTestId("link-document-1")
        .should("be.visible")
        .and("have.text", "MCLOVEMG3");
    });
  });

  describe("Opening unavailable documents", () => {
    it("Should handle opening of an unsupported fileType or content type document which returns statuscode 415", () => {
      cy.overrideRoute(FILE_ROUTE, {
        type: "break",
        httpStatusCode: 415,
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("name-text-document-1").should("not.exist");
      cy.findByTestId("view-warning-document-1").should("not.exist");
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("tab-content-1").should(
        "contain.text",
        "This document has unsupported file type or content and is unavailable."
      );
      cy.findByTestId("link-document-1").should("not.exist");
      cy.findByTestId("name-text-document-1").should("exist");
      cy.findByTestId("view-warning-document-1")
        .should("exist")
        .should("contains.text", "Document only available on CMS");
    });

    it("Should handle opening of an encrypted or password protected document which returns statuscode 403", () => {
      cy.overrideRoute(FILE_ROUTE, {
        type: "break",
        httpStatusCode: 403,
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("name-text-document-1").should("not.exist");
      cy.findByTestId("view-warning-document-1").should("not.exist");
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("tab-content-1").should(
        "contain.text",
        "This document has been encrypted or password protected and is unavailable."
      );
      cy.findByTestId("link-document-1").should("not.exist");
      cy.findByTestId("name-text-document-1").should("exist");
      cy.findByTestId("view-warning-document-1")
        .should("exist")
        .should("contains.text", "Document only available on CMS");
    });

    it("Should handle opening of a document for any other reason which returns statuscode other than 200", () => {
      cy.overrideRoute(FILE_ROUTE, {
        type: "break",
        httpStatusCode: 500,
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("name-text-document-1").should("not.exist");
      cy.findByTestId("view-warning-document-1").should("not.exist");
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("tab-content-1").should(
        "contain.text",
        "This document is unavailable"
      );
      cy.findByTestId("link-document-1").should("not.exist");
      cy.findByTestId("name-text-document-1").should("exist");
      cy.findByTestId("view-warning-document-1")
        .should("exist")
        .should("contains.text", "Document only available on CMS");
    });
  });
});

export {};
