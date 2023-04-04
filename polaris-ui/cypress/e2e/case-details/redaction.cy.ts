describe("redaction refresh flow", () => {
  it(
    "should successfully complete the redaction refresh flow for saving redaction of single document two times",
    { defaultCommandTimeout: 15000 },
    () => {
      cy.visit("/case-details/12AB1111111/13401");
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
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
      cy.findByTestId("pdfTab-spinner-0").should("not.exist");
      cy.findByTestId("div-pdfviewer-0").should("exist");
    }
  );

  it(
    "should successfully complete the redaction refresh flow for saving redaction of two different documents",
    { defaultCommandTimeout: 15000 },
    () => {
      cy.visit("/case-details/12AB1111111/13401");
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
      //save both redaction simultaneously
      cy.findByTestId("btn-save-redaction-1").click({ force: true });
      cy.findByTestId("btn-save-redaction-0").click({ force: true });
      cy.findByTestId("pdfTab-spinner-1").should("exist");
      cy.findByTestId("div-pdfviewer-1").should("not.exist");
      cy.findByTestId("btn-tab-0").click({ force: true });
      cy.findByTestId("pdfTab-spinner-0").should("exist");
      cy.findByTestId("div-pdfviewer-0").should("not.exist");
      cy.findByTestId("btn-tab-1").click({ force: true });
      cy.findByTestId("pdfTab-spinner-1").should("not.exist");
      cy.findByTestId("div-pdfviewer-1").should("exist");
      //switch to first tab and save redaction
      cy.findByTestId("btn-tab-0").click({ force: true });
      cy.findByTestId("pdfTab-spinner-0").should("not.exist");
      cy.findByTestId("div-pdfviewer-0").should("exist");
    }
  );
});
