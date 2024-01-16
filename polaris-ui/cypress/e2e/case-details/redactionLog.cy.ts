import {
  SAVE_REDACTION_ROUTE,
  SAVE_REDACTION_LOG_ROUTE,
  REDACTION_LOG_MAPPING_ROUTE,
} from "../../../src/mock-api/routes";

describe("Redaction Log", () => {
  describe("Feature Flag On", () => {
    it("Should show the redaction types select input along with the redaction button and show under redaction modal on clicking save redaction with correct redaction type summary in descending order of redaction types count", () => {
      cy.visit("/case-details/12AB1111111/13401?redactionLog=true");
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

      cy.selectPDFTextElement("Not disclosable");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Suspect 1");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("3");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("3");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("EOIN");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("3");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("4");
      cy.findByTestId("btn-redact").click();

      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("have.length", 1);
      cy.findByTestId("rl-under-redaction-content").should("have.length", 1);

      cy.get("h1").contains("99ZZ9999999 - Redaction Log").should("exist");
      cy.get("h2")
        .contains('Redaction details for:"MCLOVEMG3"')
        .should("exist");

      cy.findByTestId("redaction-summary")
        .get("li:nth-child(1)")
        .should("contain", "3 - Occupations");
      cy.findByTestId("redaction-summary")
        .get("li:nth-child(2)")
        .should("contain", "2 - Titles");
      cy.findByTestId("redaction-summary")
        .get("li:nth-child(3)")
        .should("contain", "1 - Relationship to others");
    });

    it("Should be able to open and close the guidance and show the relevant guidance text", () => {
      cy.visit("/case-details/12AB1111111/13401?redactionLog=true");
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
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("have.length", 1);
      cy.findByTestId("rl-under-redaction-content").should("have.length", 1);
      cy.findByTestId("guidance-redaction-log").click();
      cy.findByTestId("guidance-redaction-log-panel").should("exist");
      cy.findByTestId("guidance-redaction-log-panel").should(
        "contain",
        "This popup allows the capture of details which will be recorded into the Redaction Log automatically"
      );
      cy.findByTestId("btn-modal-close").click();
      cy.findByTestId("guidance-redaction-log-panel").should("not.exist");

      // cy.findByTestId("guidance-supporting-notes").click();
      // cy.findByTestId("guidance-supporting-notes-panel").should("exist");
      // cy.findByTestId("guidance-supporting-notes-panel").should(
      //   "contain",
      //   "Detail the redaction issue identified, e.g. Statement of XX (Initials) DOB redacted"
      // );
      // cy.findByTestId("btn-modal-close").click();
      // cy.findByTestId("guidance-supporting-notes-panel").should("not.exist");
    });
    it("Save and close button in the under redaction page should be disabled initially and should be enabled after redaction is saved", () => {
      cy.overrideRoute(
        SAVE_REDACTION_ROUTE,
        {
          type: "delay",
          timeMs: 500,
        },
        "put"
      );
      cy.visit("/case-details/12AB1111111/13401?redactionLog=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("be.visible");
      cy.findByTestId("rl-under-redaction-content").should("be.visible");
      cy.findByTestId("rl-under-redaction-content").contains("1 - Title");
      cy.findByTestId("rl-saved-redactions").should("not.exist");
      cy.findByTestId("rl-saving-redactions").should(
        "have.text",
        "Saving redactions..."
      );
      cy.findByTestId("btn-save-redaction-log").should("be.disabled");
      cy.findByTestId("rl-saving-redactions").should("not.exist");
      cy.findByTestId("rl-saved-redactions").should(
        "have.text",
        "Redactions successfully saved"
      );
      cy.findByTestId("btn-save-redaction-log").should("not.be.disabled");
    });
    it("Under redaction modal should throw error for empty select values and should be able to successfully save the under redaction log", () => {
      cy.overrideRoute(
        SAVE_REDACTION_ROUTE,
        {
          type: "delay",
          timeMs: 500,
        },
        "put"
      );

      cy.overrideRoute(
        REDACTION_LOG_MAPPING_ROUTE,
        {
          body: {
            businessUnits: [],
            documentTypes: [],
            investigatingAgencies: [],
          },
        },
        "get",
        Cypress.env("REACT_APP_REDACTION_LOG_BASE_URL")
      );

      cy.visit("/case-details/12AB1111111/13401?redactionLog=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("be.visible");
      cy.findByTestId("rl-under-redaction-content").should("be.visible");
      cy.get("h2")
        .contains('Redaction details for:"MCLOVEMG3"')
        .should("exist");
      cy.findByTestId("rl-under-redaction-content").contains("1 - Title");
      cy.findByTestId("btn-save-redaction-log").should("not.be.disabled");
      cy.findByTestId("btn-save-redaction-log").click();

      cy.get("#error-summary-title").should("exist");
      cy.findByTestId("redaction-log-error-summary")
        .find("li")
        .should("have.length", 3);

      cy.get("#select-cps-area-error").should("exist");
      cy.get("#select-cps-area-error").should(
        "have.text",
        "Error: Select an Area or Division"
      );
      cy.findByTestId("select-cps-area-link").should("exist");
      cy.get("#select-cps-bu-error").should("exist");
      cy.get("#select-cps-bu-error").should(
        "have.text",
        "Error: Select a Business Unit"
      );
      cy.findByTestId("select-cps-bu-link").should("exist");

      cy.get("#select-cps-dt-error").should("exist");
      cy.get("#select-cps-dt-error").should(
        "have.text",
        "Error: Select a Document Type"
      );
      cy.findByTestId("select-cps-dt-link").should("exist");
      cy.findByTestId("select-cps-area").select("1");
      cy.get("#select-cps-area-error").should("not.exist");
      cy.findByTestId("select-cps-area-link").should("not.exist");
      cy.findByTestId("select-cps-bu").select("1");
      cy.get("#select-cps-bu-error").should("not.exist");
      cy.findByTestId("select-cps-bu-link").should("not.exist");
      cy.findByTestId("select-cps-dt").select("1");
      cy.get("#select-cps-dt-error").should("not.exist");
      cy.findByTestId("select-cps-dt-link").should("not.exist");

      cy.get("#error-summary-title").should("not.exist");

      cy.findByTestId("btn-save-redaction-log").click();
      cy.findByTestId("div-modal").should("not.exist");
    });

    it("Should hide RedactionLog modal and should show error message if the saving of redaction is failed", () => {
      cy.overrideRoute(
        SAVE_REDACTION_ROUTE,
        {
          type: "break",
          httpStatusCode: 500,
          timeMs: 500,
        },
        "put"
      );
      cy.visit("/case-details/12AB1111111/13401?redactionLog=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("be.visible");
      cy.findByTestId("rl-under-redaction-content").should("be.visible");
      cy.get("h2")
        .contains('Redaction details for:"MCLOVEMG3"')
        .should("exist");
      cy.findByTestId("rl-under-redaction-content").should("not.exist");
      cy.findByTestId("div-modal")
        .should("exist")
        .contains("Failed to save redaction. Please try again later.");
      cy.findByTestId("btn-error-modal-ok").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("btn-save-redaction-0").should("not.be.disabled");
      cy.findByTestId("btn-link-removeAll-0").should("not.be.disabled");
    });
    it("Should hide RedactionLog modal and should show error message if the saving of redaction log is failed", () => {
      cy.overrideRoute(
        SAVE_REDACTION_LOG_ROUTE,
        {
          type: "break",
          httpStatusCode: 500,
          timeMs: 500,
        },
        "post",
        Cypress.env("REACT_APP_REDACTION_LOG_BASE_URL")
      );
      cy.visit("/case-details/12AB1111111/13401?redactionLog=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("be.visible");
      cy.findByTestId("rl-under-redaction-content").should("be.visible");
      cy.get("h2")
        .contains('Redaction details for:"MCLOVEMG3"')
        .should("exist");
      cy.findByTestId("rl-under-redaction-content").contains("1 - Title");
      cy.findByTestId("btn-save-redaction-log").should("not.be.disabled");
      cy.findByTestId("btn-save-redaction-log").click();
      cy.findByTestId("rl-under-redaction-content").should("not.exist");
      cy.findByTestId("div-modal")
        .should("exist")
        .contains(
          "The entries into the Redaction Log have failed. Please go to the Redaction Log and enter manually."
        );
    });
  });

  describe("Feature Flag Off", () => {
    it("Should not show the redaction types select input along with the redaction button ", () => {
      cy.visit("/case-details/12AB1111111/13401?");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("select-redaction-type").should("have.length", 0);
    });
  });
});
