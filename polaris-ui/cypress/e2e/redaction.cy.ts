import {
  SAVE_REDACTION_ROUTE,
  DOCUMENT_CHECKOUT_ROUTE,
  GET_DOCUMENTS_LIST_ROUTE,
} from "../../src/mock-api/routes";

import {
  getRefreshRedactedDocument,
  getRefreshDeletedDocuments,
} from "../../src/mock-api/data/getDocumentsList.cypress";

describe("redaction refresh flow", () => {
  it("should successfully complete the redaction refresh flow for saving redaction of single document two times", () => {
    const documentList = getRefreshRedactedDocument("1", 2);
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
      body: documentList[0],
      timeMs: 1000,
    });
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId("btn-save-redaction-0").click();
    cy.findByTestId("div-modal").should("be.visible");
    cy.findByTestId("rl-under-redaction-content").should("be.visible");
    cy.findByTestId("btn-save-redaction-log").click();
    cy.findByTestId("pdfTab-spinner-0").should("exist");
    cy.findByTestId("div-pdfviewer-0").should("not.exist");
    cy.findByTestId("pdfTab-spinner-0").should("not.exist");
    cy.findByTestId("div-pdfviewer-0").should("exist");
    cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
      body: documentList[1],
      timeMs: 1000,
    });

    //saving for the second time
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId("btn-save-redaction-0").click();
    cy.findByTestId("div-modal").should("be.visible");
    cy.findByTestId("rl-under-redaction-content").should("be.visible");
    cy.findByTestId("btn-save-redaction-log").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("pdfTab-spinner-0").should("exist");
    cy.findByTestId("div-pdfviewer-0").should("not.exist");
    cy.findByTestId("pdfTab-spinner-0").should("not.exist");
    cy.findByTestId("div-pdfviewer-0").should("exist");
  });

  it("should successfully complete the redaction refresh flow for saving redaction of two different documents", () => {
    const documentList = getRefreshRedactedDocument("1");
    cy.visit("/case-details/12AB1111111/13401?redactionLog");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
      body: documentList[0],
      timeMs: 1000,
    });
    cy.findAllByTestId("div-pdfviewer-0")
      .first()
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
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
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    //save both redaction simultaneously
    cy.findByTestId("btn-save-redaction-1").click({ force: true });
    cy.findByTestId("div-modal").should("be.visible");
    cy.findByTestId("rl-under-redaction-content").should("be.visible");
    cy.findByTestId("btn-save-redaction-log").click();
    cy.findByTestId("pdfTab-spinner-1").should("exist");
    cy.findByTestId("div-pdfviewer-1").should("not.exist");
    cy.findByTestId("pdfTab-spinner-1").should("not.exist");
    cy.findByTestId("div-pdfviewer-1").should("exist");
    cy.findByTestId("btn-save-redaction-0").click({ force: true });
    cy.findByTestId("div-modal").should("be.visible");
    cy.findByTestId("rl-under-redaction-content").should("be.visible");
    cy.findByTestId("btn-save-redaction-log").click();
    cy.findByTestId("pdfTab-spinner-0").should("not.exist");
    cy.findByTestId("div-pdfviewer-0").should("exist");
  });

  it("Should disable saveRedaction and remove all redaction button, when saving a redaction", () => {
    cy.overrideRoute(
      SAVE_REDACTION_ROUTE,
      {
        type: "delay",
        timeMs: 1000,
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
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
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
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId("btn-save-redaction-0").click();
    cy.findByTestId("btn-save-redaction-0").should("be.disabled");
    cy.findByTestId("btn-link-removeAll-0").should("be.disabled");
    cy.findByTestId("div-modal").should("be.visible");
    cy.findByTestId("rl-under-redaction-content").should("be.visible");
    cy.findByTestId("btn-save-redaction-log").should("be.disabled");
    cy.findByTestId("rl-under-redaction-content").should("not.exist");
    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to save document. Please try again.");
    cy.findByTestId("div-modal").contains(
      "Your redactions have been saved and it will be possible to re-apply them next time you open this document."
    );
    cy.findByTestId("div-modal").contains(
      "If re-trying is not successful, please notify the Casework App product team."
    );

    cy.findByTestId("btn-error-modal-ok").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("btn-save-redaction-0").should("not.be.disabled");
    cy.findByTestId("btn-link-removeAll-0").should("not.be.disabled");
  });

  it("Should handle the deleted document opened in a tab after the pipeline refresh and display document deleted message to user", () => {
    const documentList = getRefreshDeletedDocuments("1", "2");

    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-2").click();
    cy.wait(500);
    cy.findByTestId("link-document-1").click();
    cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
      body: documentList[0],
      timeMs: 1000,
    });

    cy.findByTestId("div-pdfviewer-0").should("exist");
    cy.findByTestId("div-pdfviewer-1")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByText("This document has been deleted and is unavailable.").should(
      "not.exist"
    );
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click();
    cy.findByTestId("btn-save-redaction-1").click();
    cy.findByTestId("div-modal").should("be.visible");
    cy.findByTestId("rl-under-redaction-content").should("be.visible");
    cy.findByTestId("btn-save-redaction-log").click();
    cy.findByTestId("pdfTab-spinner-1").should("exist");
    cy.findByTestId("div-pdfviewer-1").should("not.exist");
    cy.findByTestId("pdfTab-spinner-1").should("not.exist");
    cy.findByTestId("div-pdfviewer-1").should("exist");
    cy.findByTestId("btn-tab-0").click();
    cy.findByTestId("div-pdfviewer-0").should("not.exist");
    cy.findAllByText(
      "This document has been deleted and is unavailable."
    ).should("exist");
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
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
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
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to checkout document. Please try again later.");
    cy.findByTestId("btn-save-redaction-0").should("have.length", 0);
  });

  it("Should not hide the redaction tip if we hover over an unsaved redaction in the middle of doing a text selection(linear) redaction", () => {
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.selectPDFTextElement("Not disclosable");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.selectPDFTextElement("MCLOVE");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    //hovering over unsaved redaction to verify redaction tip is not removed.
    cy.get('[data-testid^="unsaved-redaction-"]').eq(0).trigger("mouseover");
    cy.findByTestId("remove-btn").should("not.exist");
    cy.findByTestId("btn-redact").should("exist");
    cy.findByTestId("btn-redact").click({ force: true });
    //after redaction verifying remove redaction tip appears on hovering over unsaved redaction
    cy.findByTestId("btn-redact").should("not.exist");
    cy.get('[data-testid^="unsaved-redaction-"]').eq(1).trigger("mouseover");
    cy.findByTestId("remove-btn").should("exist");
    cy.get('[data-testid^="unsaved-redaction-"]').eq(0).trigger("mouseover");
    cy.findByTestId("remove-btn").should("exist");
  });
});
