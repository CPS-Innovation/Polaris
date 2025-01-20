import { getUsedUnusedDocumentState } from "../../src/mock-api/data/getDocumentsList.cypress";
import { GET_DOCUMENTS_LIST_ROUTE } from "../../src/mock-api/routes";

describe("Checks if the used/unused functionality", () => {
  //   it("Should show 'Change as used/unused' document option if the document 'isUsed' presentationFlags property is true", () => {
  //     cy.visit("/case-details/12AB1111111/13401?reclassify=true");
  //     cy.findByTestId("btn-accordion-open-close-all").click();

  //     cy.findByTestId("document-housekeeping-actions-dropdown-1").should("exist");
  //     cy.findByTestId("dropdown-panel").should("not.exist");
  //     cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
  //     cy.findByTestId("dropdown-panel").contains("Reclassify document");
  //     cy.realPress("Escape");

  //     cy.findByTestId("dropdown-panel").should("not.exist");
  //     cy.findByTestId("document-housekeeping-actions-dropdown-2").should("exist");
  //     cy.findByTestId("document-housekeeping-actions-dropdown-2").click();
  //     cy.findByTestId("dropdown-panel").contains("Check as");
  //     cy.realPress("Escape");

  // cy.findByTestId('communications').get('')

  //   });

it("should change document state as used / unused on option select", () => {
    cy.visit("/case-details/12AB1111111/13401?isUsed=true");
  cy.findByTestId("btn-accordion-open-close-all").click();
  cy.findByTestId("div-reclassify").should("not.exist");
  cy.findByTestId("document-housekeeping-actions-dropdown-10").click();

  const lastElement = cy
    .findByTestId("dropdown-panel")
    .find("ul")
    .find("li")
    .eq(2);

    lastElement.should("have.text", "Mark as unused");
    lastElement.click();

    const documentList = getUsedUnusedDocumentState("10", true, 1);

  cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
    body: documentList[0],
    timeMs: 1000,
  });

    cy.visit("/case-details/12AB1111111/13401?isUsed=true");
  cy.findByTestId("btn-accordion-open-close-all").click();
  cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
  cy.findByTestId("dropdown-panel")
    .find("ul")
    .find("li")
    .eq(2)
    .should("have.text", "Mark as used");
  });
});
