import { getUsedUnusedDocumentState } from "../../src/mock-api/data/getDocumentsList.cypress";
import {
  GET_DOCUMENTS_LIST_ROUTE,
  TOGGLE_USED_DOCUMENT_ROUTE,
} from "../../src/mock-api/routes";

describe("Checks if the used/unused functionality", () => {
  xit("should change document state as used / unused on option select", () => {
    cy.visit("/case-details/42MZ7213221/13401/?isUnused=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();

    const lastElement = cy
      .findByTestId("dropdown-panel")
      .find("ul")
      .find("li")
      .eq(2);

    lastElement.should("have.text", "Mark as unused");
    lastElement.click();

    const documentList = getUsedUnusedDocumentState("10", true, 1);

    cy.findByTestId("rl-saved-redactions").should(
      "contains.text",
      "successfully saved to CMS"
    );

    cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
      body: documentList[0],
      timeMs: 1000,
    });
  });

  xit("should show an error message on failure", () => {
    cy.visit("/case-details/42MZ7213221/13401/?isUnused=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();

    cy.overrideRoute(
      TOGGLE_USED_DOCUMENT_ROUTE,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "post"
    );

    const lastElement = cy
      .findByTestId("dropdown-panel")
      .find("ul")
      .find("li")
      .eq(2);

    lastElement.click();

    cy.findByText("Something went wrong!");
    cy.findByText("Failed to change the document state.");
  });
});
