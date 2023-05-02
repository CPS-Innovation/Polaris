import {
  INITIATE_PIPELINE_ROUTE,
  SAVE_REDACTION_ROUTE,
  TRACKER_ROUTE,
} from "../../../../src/mock-api/routes";

import { refreshPipelineDeletedDocuments } from "../../../../src/mock-api/data/pipelinePdfResults.cypress";

describe("redaction refresh flow", () => {
  it("should successfully complete the redaction refresh flow for saving redaction of single document two times", () => {
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
  });

  it("should successfully complete the redaction refresh flow for saving redaction of two different documents", () => {
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
    cy.findByTestId("pdfTab-spinner-0").should("exist");
    cy.findByTestId("div-pdfviewer-0").should("not.exist");
    cy.findByTestId("pdfTab-spinner-1").should("not.exist");
    cy.findByTestId("div-pdfviewer-1").should("exist");
    cy.findByTestId("pdfTab-spinner-0").should("not.exist");
    cy.findByTestId("div-pdfviewer-0").should("exist");
  });

  it("should call again the initiate pipeline, if the previous call return 423 status during redaction refresh flow and successfully complete the redaction refresh flow", () => {
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").click({ force: true });
    cy.overrideRoute(
      INITIATE_PIPELINE_ROUTE,
      {
        type: "break",
        httpStatusCode: 423,
        body: {
          trackerUrl:
            "https://mocked-out-api/api/urns/12AB1111111/cases/13401/tracker",
        },
      },
      "post"
    );

    cy.findByTestId("btn-save-redaction-0").click();
    cy.findByTestId("pdfTab-spinner-0").should("exist");
    cy.findByTestId("div-pdfviewer-0").should("not.exist");
    cy.findByTestId("pdfTab-spinner-0").should("not.exist");
    cy.findByTestId("div-pdfviewer-0").should("exist");
  });

  it("should call again the initiate pipeline, if the previous call return 423 status successfully load the documents on the initial load", () => {
    cy.overrideRoute(
      INITIATE_PIPELINE_ROUTE,
      {
        type: "break",
        httpStatusCode: 423,
        body: {
          trackerUrl:
            "https://mocked-out-api/api/urns/12AB1111111/cases/13401/tracker",
        },
      },
      "post"
    );
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
  });

  it("Should show error message when failed to save the redaction", () => {
    cy.overrideRoute(
      SAVE_REDACTION_ROUTE,
      {
        type: "break",
        httpStatusCode: 500,
      },
      "put"
    );
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
    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to save redaction. Please try again later.");
    cy.findByTestId("btn-error-modal-ok").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("btn-save-redaction-0").should("exist");
  });
  it("Should handle the deleted document opened in a tab after the pipeline refresh and display document deleted message to user", () => {
    cy.overrideRoute(TRACKER_ROUTE, {
      body: refreshPipelineDeletedDocuments()[0],
    });
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-2").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0").should("exist");
    cy.findByTestId("div-pdfviewer-1")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByText("This document has been deleted and is unavailable.").should(
      "not.exist"
    );
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").click();
    cy.findByTestId("btn-save-redaction-1").click();
    cy.overrideRoute(TRACKER_ROUTE, {
      body: refreshPipelineDeletedDocuments()[1],
    });
    cy.findByTestId("pdfTab-spinner-1").should("exist");
    cy.findByTestId("div-pdfviewer-1").should("not.exist");
    cy.findByTestId("pdfTab-spinner-1").should("not.exist");
    cy.findByTestId("div-pdfviewer-1").should("exist");
    cy.findByTestId("btn-tab-0").click();
    cy.findByTestId("div-pdfviewer-0").should("not.exist");
    cy.findByText("This document has been deleted and is unavailable.").should(
      "exist"
    );
  });
});
