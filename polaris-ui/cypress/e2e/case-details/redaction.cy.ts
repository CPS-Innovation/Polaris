import {
  INITIATE_PIPELINE_ROUTE,
  SAVE_REDACTION_ROUTE,
  TRACKER_ROUTE,
  DOCUMENT_CHECKOUT_ROUTE,
} from "../../../src/mock-api/routes";

import { refreshPipelineDeletedDocuments } from "../../../src/mock-api/data/pipelinePdfResults.cypress";

xdescribe("redaction refresh flow", () => {
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
    cy.wait(500);
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
        body: JSON.stringify({
          trackerUrl:
            "https://mocked-out-api/api/urns/12AB1111111/cases/13401/tracker",
        }),
      },
      "post"
    );

    const initiatePipelineRequestCounter = { count: 0 };
    cy.trackRequestCount(
      initiatePipelineRequestCounter,
      "POST",
      "/api/urns/12AB1111111/cases/13401"
    );
    cy.findByTestId("btn-save-redaction-0").click();
    cy.findByTestId("pdfTab-spinner-0").should("exist");
    cy.findByTestId("div-pdfviewer-0").should("not.exist");
    cy.findByTestId("pdfTab-spinner-0").should("not.exist");
    cy.findByTestId("div-pdfviewer-0").should("exist");
    cy.window().then(() => {
      expect(initiatePipelineRequestCounter.count).to.equal(2);
    });
  });

  it("should not call again the initiate pipeline, if the previous call return 423 status  and should successfully load the documents on the initial load", () => {
    const initiatePipelineRequestCounter = { count: 0 };
    cy.trackRequestCount(
      initiatePipelineRequestCounter,
      "POST",
      "/api/urns/12AB1111111/cases/13401"
    );
    cy.overrideRoute(
      INITIATE_PIPELINE_ROUTE,
      {
        type: "break",
        httpStatusCode: 423,
        body: JSON.stringify({
          trackerUrl:
            "https://mocked-out-api/api/urns/12AB1111111/cases/13401/tracker",
        }),
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
    cy.window().then(() => {
      expect(initiatePipelineRequestCounter.count).to.equal(1);
    });
  });

  it("Should disable saveRedaction and remove all redaction button, when saving a redaction", () => {
    cy.overrideRoute(
      SAVE_REDACTION_ROUTE,
      {
        type: "delay",
        timeMs: 3000,
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
    cy.findByTestId("btn-save-redaction-0").should("be.disabled");
    cy.findByTestId("btn-link-removeAll-0").should("be.disabled");
  });
  it("Should show error message when failed to save the redaction and should enable back the save redaction button", () => {
    cy.overrideRoute(
      SAVE_REDACTION_ROUTE,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 3000,
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
    cy.findByTestId("btn-save-redaction-0").should("be.disabled");
    cy.findByTestId("btn-link-removeAll-0").should("be.disabled");
    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to save redaction. Please try again later.");
    cy.findByTestId("btn-error-modal-ok").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("btn-save-redaction-0").should("not.be.disabled");
    cy.findByTestId("btn-link-removeAll-0").should("not.be.disabled");
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
  it("should show correct error message on unsuccessful checkout, due to the document is locked by another user", () => {
    cy.overrideRoute(
      DOCUMENT_CHECKOUT_ROUTE,
      {
        type: "break",
        httpStatusCode: 409,
        body: "test_user_name",
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
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId("div-modal")
      .should("exist")
      .contains(
        `It is not possible to redact as the document is already checked out by test_user_name. Please try again later.`
      );
    cy.findByTestId("btn-save-redaction-0").should("have.length", 0);
  });
  it("should show general error message on unsuccessful checkout, due to any reason other than document is locked by another user", () => {
    cy.overrideRoute(
      DOCUMENT_CHECKOUT_ROUTE,
      {
        type: "break",
        httpStatusCode: 500,
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
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to checkout document. Please try again later.");
    cy.findByTestId("btn-save-redaction-0").should("have.length", 0);
  });
});
