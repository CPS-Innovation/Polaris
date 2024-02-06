/// <reference types="cypress" />

const {
  HAPPY_PATH_URN,
  HAPPY_PATH_TARGET_DEFENDANT_NAME,
  HAPPY_PATH_TARGET_DOCUMENT_NAME,
  HAPPY_PATH_TARGET_SEARCH_TEXT,
  HAPPY_PATH_CASE_ID,
  PRE_SEARCH_DELAY_MS,
} = Cypress.env();

describe("Happy Path", { tags: '@ci' }, () => {
  it("can view a case", () => {
    cy.on("uncaught:exception", () => false);

    cy.fullLogin();

    cy.clearCaseTracker(HAPPY_PATH_URN, HAPPY_PATH_CASE_ID);
    cy.visit("/polaris-ui");

    cy.findByTestId("input-search-urn").type(`${HAPPY_PATH_URN}{enter}`);

    // open case details page
    cy.findByTestId(`link-${HAPPY_PATH_URN}`).click();

    // is our defendant correct
    cy.findByTestId("txt-defendant-name").contains(
      HAPPY_PATH_TARGET_DEFENDANT_NAME
    );

    // open our target document
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByText(HAPPY_PATH_TARGET_DOCUMENT_NAME).click();

    cy.wait(PRE_SEARCH_DELAY_MS);

    // search for our target text
    cy.findByTestId("input-search-case").type(
      `${HAPPY_PATH_TARGET_SEARCH_TEXT}{enter}`
    );
    cy.get("#modal").findByText(HAPPY_PATH_TARGET_DOCUMENT_NAME).click();

    // is our target document correct - has expected fragment in search header
    cy.findByTestId("tabs").contains(
      `for "${HAPPY_PATH_TARGET_SEARCH_TEXT}" in ${HAPPY_PATH_TARGET_DOCUMENT_NAME}`
    );

    // close the document tab
    cy.findByTestId("tab-remove").click();

    // have all docs processed ok?
    // (do this last to give the indexing time to work while we assert the above)
    cy.findByTestId("span-flag-all-indexed");
  });
});

export {};
