/// <reference types="cypress" />

const {
  TARGET_URN,
  TARGET_DEFENDANT_NAME,
  TARGET_DOCUMENT_NAME,
  TARGET_DOCUMENT_TEXT_FRAGMENT,
  TARGET_SEARCH_TEXT,
  TARGET_CASE_ID,
  PRE_SEARCH_DELAY_MS,
} = Cypress.env();

describe("Happy Path", () => {
  it("can view a case", () => {
    cy.on("uncaught:exception", () => false);

    cy.fullLogin();

    cy.clearCaseTracker(TARGET_URN, TARGET_CASE_ID);
    cy.visit("/polaris-ui");

    cy.findByTestId("input-search-urn").type(`${TARGET_URN}{enter}`);

    // open case details page
    cy.findByTestId(`link-${TARGET_URN}`).click();

    // is our defendant correct
    cy.findByTestId("txt-defendant-name").contains(TARGET_DEFENDANT_NAME);

    // open our target document
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByText(TARGET_DOCUMENT_NAME).click();

    // is our target document correct - has expected fragment
    // cy.findByTestId("btn-open-pdf").contains(TARGET_DOCUMENT_NAME)
    // cy.get("span").contains(TARGET_DOCUMENT_TEXT_FRAGMENT)

    cy.wait(PRE_SEARCH_DELAY_MS);

    // search for our target text
    cy.findByTestId("input-search-case").type(`${TARGET_SEARCH_TEXT}{enter}`);
    cy.get("#modal").findByText(TARGET_DOCUMENT_NAME).click();

    // is our target document correct - has expected fragment in search header
    cy.findByTestId("tabs").contains(
      `for "${TARGET_SEARCH_TEXT}" in ${TARGET_DOCUMENT_NAME}`
    );

    // close the document tab
    cy.findByTestId("tab-remove").click();

    // have all docs processed ok?
    // (do this last to give the indexing time to work while we assert the above)
    cy.findByTestId("span-flag-all-indexed");
  });
});

export {};
