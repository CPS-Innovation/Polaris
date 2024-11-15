import {
  RENAME_DOCUMENT_ROUTE,
  GET_DOCUMENTS_LIST_ROUTE,
} from "../../src/mock-api/routes";

import { getRefreshRenamedDocuments } from "../../src/mock-api/data/getDocumentsList.cypress";
describe("Feature Rename Document", () => {
  it("Should show rename document option if the document 'canRename' is true and should not show if it is not ", () => {
    cy.visit("/case-details/12AB1111111/13401?renameDocument=true");
    cy.findByTestId("btn-accordion-open-close-all").click(); //12;
    cy.findByTestId("document-housekeeping-actions-dropdown-1").should("exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-2").should("exist");
    cy.findByTestId("dropdown-panel").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-2").click();
    cy.findByTestId("dropdown-panel").contains("Rename document");
    cy.findByTestId("document-housekeeping-actions-dropdown-12").should(
      "not.exist"
    );
  });

  it("Should show the rename panel and content correctly and be able to close the rename panel with close and cancel button and should put back the focus correctly on closing the panel", () => {
    cy.visit("/case-details/12AB1111111/13401?renameDocument=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("rename-panel").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Rename document").click();
    cy.findByTestId("rename-panel").contains("Rename - PortraitLandscape");
    cy.findByTestId("rename-panel").contains(
      "What is the new name of your document?"
    );
    cy.findByTestId("rename-panel")
      .contains("What is the new name of your document?")
      .click();
    cy.focused().should("have.id", "rename-text-input");
    cy.findByTestId("rename-text-input").should(
      "have.value",
      "PortraitLandscape"
    );
    cy.findByTestId("btn-close-rename-panel").click();
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Rename document").click();
    cy.findByTestId("rename-panel").contains("Rename - PortraitLandscape");
    cy.findByTestId("btn-cancel-rename").click();
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
  });

  it("Should successfully rename the document and do the updates correctly after successful rename", () => {
    const documentList = getRefreshRenamedDocuments(
      "10",
      "PortraitLandscape_1",
      1
    );

    const expectedSaveRenamePayload = {
      documentId: 10,
      documentName: "PortraitLandscape_1",
    };
    const saveRenameRequestObject = { body: "" };
    cy.trackRequestBody(
      saveRenameRequestObject,
      "PUT",
      "/api/urns/12AB1111111/cases/13401/documents/10/rename"
    );

    cy.visit("/case-details/12AB1111111/13401?renameDocument=true");

    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId("link-document-10")
      .should("be.visible")
      .and("have.text", "PortraitLandscape");
    cy.findByTestId("link-document-10").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("Page1 Portrait");

    cy.findByTestId("rename-panel").should("not.exist");
    cy.findByTestId("link-document-10").should(
      "have.text",
      "PortraitLandscape"
    );
    cy.findByTestId("tab-active").should("have.text", "PortraitLandscape");

    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Rename document").click();
    cy.findByTestId("rename-panel").should("exist");

    cy.findByTestId("rename-panel").contains("Rename - PortraitLandscape");
    cy.waitUntil(() => cy.findByTestId("rename-text-input")).then(() =>
      cy.findByTestId("rename-text-input").type("_1")
    );
    cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
      body: documentList[0],
      timeMs: 100,
    });
    cy.findByTestId("btn-save-rename").click();
    cy.findByTestId("rename-panel").contains("Saving renamed document to CMS");

    //assertion on the add note request
    cy.waitUntil(() => {
      return saveRenameRequestObject.body;
    }).then(() => {
      expect(saveRenameRequestObject.body).to.deep.equal(
        JSON.stringify(expectedSaveRenamePayload)
      );
    });

    cy.findByTestId("rename-panel").contains(
      "Document renamed successfully saved to CMS"
    );
    cy.focused().should("have.attr", "data-testid", "btn-close-rename");
    cy.findByTestId("btn-close-rename").click();
    cy.findByTestId("notes-panel").should("not.exist");
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
    cy.findByTestId("link-document-10").should(
      "have.text",
      "PortraitLandscape_1"
    );
    cy.findByTestId("tab-active").should("have.text", "PortraitLandscape_1");
  });

  it("Should show all the UI validation errors", () => {
    cy.visit("/case-details/12AB1111111/13401?renameDocument=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Rename document").click();
    cy.findByTestId("rename-panel").should("exist");
    cy.findByTestId("rename-panel").contains("Rename - PortraitLandscape");
    //same name validation
    cy.findByTestId("rename-text-input").should(
      "have.value",
      "PortraitLandscape"
    );
    cy.findByTestId("btn-save-rename").click();
    cy.findByTestId("rename-error-summary").find("li").should("have.length", 1);
    cy.findByTestId("rename-text-input-link").should(
      "have.text",
      "New name should be different from current name"
    );
    cy.findByTestId("rename-text-input-link").click();
    cy.focused().should("have.id", "rename-text-input");
    cy.get("#rename-text-input-error").should(
      "have.text",
      "Error: New name should be different from current name"
    );
    //empty value validation
    cy.waitUntil(() => cy.findByTestId("rename-text-input")).then(() =>
      cy.findByTestId("rename-text-input").clear()
    );
    cy.findByTestId("btn-save-rename").click();
    cy.findByTestId("rename-error-summary").find("li").should("have.length", 1);
    cy.findByTestId("rename-text-input-link").should(
      "have.text",
      "Enter a new name"
    );
    cy.get("#rename-text-input-error").should(
      "have.text",
      "Error: Enter a new name"
    );

    const maxLengthText =
      "New name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be 12345";
    cy.findByTestId("rename-text-input").type(maxLengthText);
    cy.realPress(".");
    cy.findByTestId("btn-save-rename").click();
    cy.findByTestId("rename-error-summary").find("li").should("have.length", 1);
    cy.findByTestId("rename-text-input-link").should(
      "have.text",
      "New name must be 252 characters or less"
    );
    cy.get("#rename-text-input-error").should(
      "have.text",
      "Error: New name must be 252 characters or less"
    );
    cy.findByTestId("rename-text-input-link").click();
    cy.realPress("{backspace}");
    cy.findByTestId("btn-save-rename").click();
    cy.get("#rename-text-input-error").should("not.exist");
    cy.findByTestId("rename-error-summary").should("not.exist");
  });

  it("Should show error message if renaming a document failed, and shouldn't call refreshPipeline and tracker", () => {
    cy.overrideRoute(
      RENAME_DOCUMENT_ROUTE,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "put"
    );

    cy.visit("/case-details/12AB1111111/13401?renameDocument=true");
    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId("rename-panel").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Rename document").click();
    cy.findByTestId("rename-panel").should("exist");
    cy.findByTestId("rename-panel").contains("Rename - PortraitLandscape");
    cy.waitUntil(() => cy.findByTestId("rename-text-input")).then(() =>
      cy.findByTestId("rename-text-input").type("_1")
    );
    cy.findByTestId("btn-save-rename").click();
    cy.findByTestId("rename-panel").contains("Saving renamed document to CMS");
    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to rename the document. Please try again.");
    cy.findByTestId("btn-error-modal-ok").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("rename-text-input").should(
      "have.value",
      "PortraitLandscape_1"
    );
    cy.findByTestId("btn-cancel-rename").click();
    cy.findByTestId("rename-panel").should("not.exist");
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
  });
});
