import { getUsedUnusedDocumentState } from "../../src/mock-api/data/getDocumentsList.cypress";
import { GET_DOCUMENTS_LIST_ROUTE } from "../../src/mock-api/routes";

it("should change document state as used / unused on option select", () => {
  cy.visit("/case-details/12AB1111111/13401?reclassify=true");
  cy.findByTestId("btn-accordion-open-close-all").click();
  cy.findByTestId("div-reclassify").should("not.exist");
  cy.findByTestId("document-housekeeping-actions-dropdown-10").click();

  const lastElement = cy
    .findByTestId("dropdown-panel")
    .find("ul")
    .find("li")
    .eq(2);

  lastElement.should("have.text", "Mark as unused").click();

  const documentList = getUsedUnusedDocumentState("10", true, 1);

  cy.overrideRoute(GET_DOCUMENTS_LIST_ROUTE, {
    body: documentList[0],
    timeMs: 1000,
  });

  cy.visit("/case-details/12AB1111111/13401?reclassify=true");
  cy.findByTestId("btn-accordion-open-close-all").click();
  cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
  cy.findByTestId("dropdown-panel")
    .find("ul")
    .find("li")
    .eq(2)
    .should("have.text", "Mark as used");
});
