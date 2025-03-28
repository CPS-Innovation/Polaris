import { redactionRequestAssertionValidator } from "../utils/redactionAssuranceUtils";
import { GET_DOCUMENTS_LIST_ROUTE } from "../../src/mock-api/routes";
import { getDocumentsListResult } from "../../src/mock-api/data/getDocumentsList.cypress";
describe("Save User Data", () => {
  describe("Read/Unread Documents", () => {
    beforeEach(() => {
      const documentList = getDocumentsListResult(1);
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[0],
      });
    });
    it("Should identify the document as read if the user has opened the document and should persist that state when user comes back in a new session and clear it if we clear local storage", () => {
      cy.clearLocalStorage();
      cy.visit("/case-details/12AB1111111/13401");
      cy.window().then((window) => {
        const storedData = window.localStorage.getItem("polaris-13401");
        expect(storedData).to.equal(null);
      });
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-10")
        .closest("li")
        .should("have.attr", "data-read", "false");
      cy.findByTestId("link-document-10").click();

      cy.findByTestId("link-document-10")
        .closest("li")
        .should("have.attr", "data-read", "true");

      cy.findByTestId("link-document-2")
        .closest("li")
        .should("have.attr", "data-read", "false");
      cy.findByTestId("link-document-2").click();
      cy.findByTestId("link-document-2")
        .closest("li")
        .should("have.attr", "data-read", "true");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("tab-remove").click();

      cy.findByTestId("link-document-10")
        .closest("li")
        .should("have.attr", "data-read", "true");
      cy.findByTestId("link-document-2")
        .closest("li")
        .should("have.attr", "data-read", "true");
      cy.window().then((win) => {
        win.sessionStorage.clear();
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.window().then((window) => {
        const storedData = JSON.parse(
          window.localStorage.getItem("polaris-13401")!
        );
        expect(storedData?.readUnread).to.deep.equal(["10", "2"]);
      });
      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("link-document-10")
        .closest("li")
        .should("have.attr", "data-read", "true");
      cy.findByTestId("link-document-2")
        .closest("li")
        .should("have.attr", "data-read", "true");
      cy.findByTestId("link-document-1").click();
      cy.window().then((window) => {
        const storedData = JSON.parse(
          window.localStorage.getItem("polaris-13401")!
        );
        expect(storedData?.readUnread).to.deep.equal(["10", "2", "1"]);
      });

      cy.clearLocalStorage();
      cy.window().then((win) => {
        win.sessionStorage.clear();
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-10")
        .closest("li")
        .should("have.attr", "data-read", "false");
      cy.findByTestId("link-document-2")
        .closest("li")
        .should("have.attr", "data-read", "false");
    });
  });

  describe("unsaved redactions", () => {
    beforeEach(() => {
      const documentList = getDocumentsListResult(1);
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[0],
      });
      cy.clearLocalStorage();
      cy.window().then((win) => {
        win.sessionStorage.clear();
      });
    });
    it("Should be able to apply and ignore unsaved redaction data if the user chose new sessions by closing the tab in the middle of redaction", () => {
      const doc10CheckoutCounter = { count: 0 };
      cy.trackRequestCount(
        doc10CheckoutCounter,
        "POST",
        /\/api\/urns\/12AB1111111\/cases\/13401\/documents\/10\/versions\/(\d+)\/checkout/
      );
      const doc1CheckoutCounter = { count: 0 };
      cy.trackRequestCount(
        doc1CheckoutCounter,
        "POST",
        /\/api\/urns\/12AB1111111\/cases\/13401\/documents\/1\/versions\/(\d+)\/checkout/
      );

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
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.window().then(() => {
        expect(doc1CheckoutCounter.count).to.equal(1);
      });
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("Page1 Portrait");
      cy.selectPDFTextElement("p1");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p10");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p11");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("redaction-count-text-1").contains(
        "There are 3 redactions"
      );
      cy.window().then(() => {
        expect(doc10CheckoutCounter.count).to.equal(1);
      });

      //comeback in a new session and apply
      cy.window().then((win) => {
        win.sessionStorage.clear();
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();

      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 2 unsaved redactions on this document, select OK to apply them."
      );

      cy.findAllByTestId("unsaved-redactions-description").contains(
        "If you do not want to apply the unsaved redactions, select Ignore. Please note: It will not be possible to recover the unsaved redactions if you select this option."
      );

      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findAllByTestId("btn-apply-redaction").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.window().then(() => {
        expect(doc1CheckoutCounter.count).to.equal(2);
      });
      //close the tab,comeback and apply
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("link-document-1").click();

      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 2 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findAllByTestId("btn-apply-redaction").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.window().then(() => {
        expect(doc1CheckoutCounter.count).to.equal(3);
      });
      //close the tab, come back and ignore
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("link-document-1").click();

      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 2 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findAllByTestId("btn-ignore-redaction").click();
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      //checkout counter wont update this time as we have not applied the redaction
      cy.window().then(() => {
        expect(doc1CheckoutCounter.count).to.equal(3);
      });
      //second document
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("Page1 Portrait");
      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 3 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-1").should("not.exist");
      cy.findAllByTestId("btn-apply-redaction").click();
      cy.findByTestId("redaction-count-text-1").contains(
        "There are 3 redactions"
      );
      cy.window().then(() => {
        expect(doc10CheckoutCounter.count).to.equal(2);
      });
      //close the tab,comeback and apply
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("Page1 Portrait");
      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 3 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-1").should("not.exist");
      cy.findAllByTestId("btn-apply-redaction").click();
      cy.findByTestId("redaction-count-text-1").contains(
        "There are 3 redactions"
      );
      cy.window().then(() => {
        expect(doc10CheckoutCounter.count).to.equal(3);
      });
      //close the tab, come back and ignore
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("Page1 Portrait");
      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 3 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-1").should("not.exist");
      cy.findAllByTestId("btn-ignore-redaction").click();
      cy.findByTestId("redaction-count-text-1").should("not.exist");
      //checkout counter wont update this time as we have not applied the redaction
      cy.window().then(() => {
        expect(doc10CheckoutCounter.count).to.equal(3);
      });
    });
    it("Should be able to apply unsaved redactions between new sessions, closing and opening of the document tab, and should be able continue adding more redactions until we successfully save the redactions ", () => {
      const expectedSaveRedactionPayload = {
        documentId: "1",
        redactions: [
          {
            pageIndex: 1,
            height: 1272.81,
            width: 900,
            redactionCoordinates: [
              { x1: 362.28, y1: 1250.66, x2: 558.32, y2: 1232.94 },
              { x1: 151.29, y1: 1005.39, x2: 214.05, y2: 987.67 },
              { x1: 351.04, y1: 975.82, x2: 383.71, y2: 958.09 },
              { x1: 185.94, y1: 303.67, x2: 237.93, y2: 285.95 },
            ],
          },
        ],
        documentModifications: [],
      };
      const saveRequestObject = { body: "" };
      cy.trackRequestBody(
        saveRequestObject,
        "PUT",
        "/api/urns/12AB1111111/cases/13401/documents/1/versions/1/redact"
      );
      const doc1CheckoutCounter = { count: 0 };
      cy.trackRequestCount(
        doc1CheckoutCounter,
        "POST",
        /\/api\/urns\/12AB1111111\/cases\/13401\/documents\/1\/versions\/(\d+)\/checkout/
      );

      const documentList = getDocumentsListResult(2);
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[0],
      });

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
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.window().then(() => {
        expect(doc1CheckoutCounter.count).to.equal(1);
      });
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[0],
      });
      cy.window().then((win) => {
        win.sessionStorage.clear();
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 2 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findAllByTestId("btn-apply-redaction").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.window().then(() => {
        expect(doc1CheckoutCounter.count).to.equal(2);
      });
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      //close the document and open it again
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");

      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 4 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findAllByTestId("btn-apply-redaction").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 4 redactions"
      );
      cy.window().then(() => {
        expect(doc1CheckoutCounter.count).to.equal(3);
      });
      cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
        body: documentList[1],
      });
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => saveRequestObject.body).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("btn-save-redaction-log").click();
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findAllByTestId("div-modal").should("not.exist");
    });
    it("Clearing of the localstorage should clear the unsaved redactions data on a new session", () => {
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
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("You have unsaved redactions");
      cy.findByTestId("btn-nav-ignore").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.findAllByTestId("div-modal").should("exist");
      cy.findAllByTestId("unsaved-redactions-description").contains(
        "You have 2 unsaved redactions on this document, select OK to apply them."
      );
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findAllByTestId("btn-apply-redaction").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );

      // clear local storage and check
      cy.clearLocalStorage();
      cy.window().then((win) => {
        win.sessionStorage.clear();
      });
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findAllByTestId("div-modal").should("not.exist");
      cy.findByTestId("redaction-count-text-0").should("not.exist");
    });
    it("Should add unsaved redactions data to the localstorage in the correct format", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.window().then((window) => {
        const storedData = window.localStorage.getItem("polaris-13401");
        expect(storedData).to.equal(null);
      });

      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.window().then((window) => {
        const storedData = JSON.parse(
          window.localStorage.getItem("polaris-13401")!
        );
        expect(storedData?.redactions?.length).to.equal(1);
        expect(storedData?.redactions[0].documentId).to.equal("1");
        expect(storedData?.redactions[0].redactionHighlights.length).to.equal(
          2
        );
      });

      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("Page1 Portrait");
      cy.selectPDFTextElement("p1");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p10");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p11");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("redaction-count-text-1").contains(
        "There are 3 redactions"
      );
      cy.window().then((window) => {
        const storedData = JSON.parse(
          window.localStorage.getItem("polaris-13401")!
        );
        expect(storedData?.redactions?.length).to.equal(2);
        expect(storedData?.redactions[0].documentId).to.equal("1");
        expect(storedData?.redactions[0].redactionHighlights.length).to.equal(
          2
        );
        expect(storedData?.redactions[1].documentId).to.equal("10");
        expect(storedData?.redactions[1].redactionHighlights.length).to.equal(
          3
        );
      });
      cy.findByTestId("btn-link-removeAll-1").click();
      cy.findByTestId("redaction-count-text-1").should("not.exist");
      cy.window().then((window) => {
        const storedData = JSON.parse(
          window.localStorage.getItem("polaris-13401")!
        );
        expect(storedData?.redactions?.length).to.equal(1);
        expect(storedData?.redactions[0].documentId).to.equal("1");
        expect(storedData?.redactions[0].redactionHighlights.length).to.equal(
          2
        );
      });
      cy.findByTestId("btn-tab-0").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.findByTestId("btn-link-removeAll-0").click();
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.window().then((window) => {
        const storedData = JSON.parse(
          window.localStorage.getItem("polaris-13401")!
        );
        expect(storedData?.redactions).to.equal(undefined);
        expect(storedData?.readUnread).to.deep.equal(["1", "10"]);
      });
    });
  });
});
