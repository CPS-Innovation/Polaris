function terminalLog(violations: any) {
  cy.task(
    "log",
    `${violations.length} accessibility violation${
      violations.length === 1 ? "" : "s"
    } ${violations.length === 1 ? "was" : "were"} detected`
  );
  // pluck specific keys to keep the table readable
  const violationData = violations.map(
    ({
      id,
      impact,
      description,
      nodes,
    }: {
      id: any;
      impact: any;
      description: any;
      nodes: any;
    }) => ({
      id,
      impact,
      description,
      nodes: nodes.length,
    })
  );

  cy.task("table", violationData);
}

describe.only("Accessibility testing using cypress-axe", () => {
  describe("The case details page should meet WCAG2.2AA standards", () => {
    beforeEach(() => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.injectAxe();
    });

    it("Has no detectable a11y violations on load", () => {
      // Test the page at initial load

      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.findByTestId("link-document-2").click();
      cy.findByTestId("div-pdfviewer-1")
        .should("exist")
        .contains("CASE OUTLINE");

      cy.checkA11y({ exclude: [".PdfHighlighter"] }, undefined, terminalLog);
    });

    it("Has no violations on search modal", () => {
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.findByTestId("input-search-case").type("drink{enter}");
      cy.findByTestId("div-search-result-1");
      cy.checkA11y({ exclude: [".pdfViewer"] }, undefined, terminalLog);
    });

    it("Has no violations on viewing search results", () => {
      cy.findByTestId("input-search-case").type("drink{enter}");
      cy.findByTestId("link-result-document-3").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("Officer’s certification");
      cy.wait(100);
      cy.checkA11y({ exclude: [".pdfViewer"] }, undefined, terminalLog);
    });

    it("Has no violations while redacting documents", () => {
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findAllByTestId("div-pdfviewer-0")
        .first()
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click({ force: true });
      //open the second document and save redaction
      cy.findByTestId("link-document-4").click();
      cy.findAllByTestId("div-pdfviewer-1")
        .last()
        .should("exist")
        .contains("CASE FILE EVIDENCE and INFORMATION ");
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click({ force: true });
      cy.checkA11y({ exclude: [".pdfViewer"] }, undefined, terminalLog);
    });

    it("Has no violations unsaved redaction popup", () => {
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("link-back-link").click();
      cy.findByTestId("div-modal").then(($modal) => {
        cy.wrap($modal).contains("You have 1 document with unsaved redactions");
        //checks for the pdf link in the modal and clicking on the link
        cy.wrap($modal)
          .findByTestId("link-document-1")
          .contains("MCLOVEMG3")
          .click();
      });
      cy.checkA11y({ exclude: [".pdfViewer"] }, undefined, terminalLog);
    });

    it("has no violations while saving a redaction", () => {
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click({ force: true });
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("pdfTab-spinner-0").should("exist");
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
      cy.findByTestId("pdfTab-spinner-0").should("not.exist");
      cy.findByTestId("div-pdfviewer-0").should("exist");

      //saving for the second time
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").should("have.length", 1);
      cy.findByTestId("btn-redact").click({ force: true });
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("pdfTab-spinner-0").should("exist");
      cy.checkA11y({ exclude: [".pdfViewer"] }, undefined, terminalLog);
    });
  });
});
