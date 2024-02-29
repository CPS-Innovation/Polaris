import { redactionRequestAssertionValidator } from "../utils/redactionAssuranceUtils";

describe("Save User Data", () => {
  describe("Read/Unread Documents", () => {
    it("Should identify the document as read if the user has opened the document and should persist that state when user comes back and clear it if we clear local storage", () => {
      cy.clearLocalStorage();
      cy.visit("/case-details/12AB1111111/13401");
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
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();

      cy.findByTestId("link-document-10")
        .closest("li")
        .should("have.attr", "data-read", "true");
      cy.findByTestId("link-document-2")
        .closest("li")
        .should("have.attr", "data-read", "true");

      cy.clearLocalStorage();
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
    it("Should store the unsaved redactions for each document if the user fail to save the redactions and need to refresh the page and clearing of local storage should clear it", () => {
      cy.clearLocalStorage();
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
      //refresh and comeback
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("Page1 Portrait");
      cy.findByTestId("redaction-count-text-1").contains(
        "There are 3 redactions"
      );
      //clear local storage and check
      cy.clearLocalStorage();
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("Page1 Portrait");
      cy.findByTestId("redaction-count-text-1").should("not.exist");
    });
    it("Should be able to continue with locally saved redactions and add more redactions", () => {
      const expectedSaveRedactionPayload = {
        documentId: "1",
        redactions: [
          {
            pageIndex: 1,
            height: 1272.81,
            width: 900,
            redactionCoordinates: [
              { x1: 362.28, y1: 1250.66, x2: 562.65, y2: 1232.94 },
              { x1: 151.29, y1: 1005.39, x2: 219.35, y2: 987.67 },
              { x1: 351.04, y1: 975.82, x2: 388.08, y2: 958.09 },
              { x1: 185.94, y1: 303.67, x2: 242.22, y2: 285.95 },
            ],
          },
        ],
      };
      const saveRequestObject = { body: "" };
      cy.trackRequestBody(
        saveRequestObject,
        "PUT",
        "/api/urns/12AB1111111/cases/13401/documents/1"
      );
      cy.clearLocalStorage();
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
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 2 redactions"
      );
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
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });
  });
});
