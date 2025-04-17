/// <reference types="cypress" />

const {
  HAPPY_PATH_URN,
  HAPPY_PATH_TARGET_DEFENDANT_NAME,
  HAPPY_PATH_TARGET_DOCUMENT_NAME,
  HAPPY_PATH_TARGET_SEARCH_TEXT,
  HAPPY_PATH_CASE_ID,
  PRE_SEARCH_DELAY_MS,
} = Cypress.env();

describe("Happy Path", { tags: ["@ci", "@ci-chunk-1"] }, () => {
  it("can view a case", () => {
    cy.on("uncaught:exception", () => false);

    cy.fullLogin();

    cy.clearCaseTracker(HAPPY_PATH_URN, HAPPY_PATH_CASE_ID);
    cy.visit("/polaris-ui");

    cy.findByTestId("input-search-urn").type(`${HAPPY_PATH_URN}{enter}`);

    // open case details page
    cy.findByTestId(`link-${HAPPY_PATH_URN}`).click();

    // is our defendant correct
    cy.findByTestId("defendant-name").contains(
      HAPPY_PATH_TARGET_DEFENDANT_NAME
    );

    // open our target document
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByText(HAPPY_PATH_TARGET_DOCUMENT_NAME).click();

    cy.wait(PRE_SEARCH_DELAY_MS);

    cy.intercept('GET', '**/search/?query=Multi').as('getSearchResults');

    // search for our target text
    cy.findByTestId("input-search-case").type(
      `${HAPPY_PATH_TARGET_SEARCH_TEXT}{enter}`
    );

    cy.wait('@getSearchResults', { timeout: 100000 }).then(() => {
      cy.findByTestId('search-results-available-link').click();
    });

    cy.get("#modal").findByText(HAPPY_PATH_TARGET_DOCUMENT_NAME).click();

    // is our target document correct - has expected fragment in search header
    cy.findByTestId("tabs").contains(
      `for "${HAPPY_PATH_TARGET_SEARCH_TEXT}" in ${HAPPY_PATH_TARGET_DOCUMENT_NAME}`
    );

    // close the document tab
    cy.findByTestId("tab-remove").click();

    //verify that under/over redaction log feature is available
    cy.findByText(HAPPY_PATH_TARGET_DOCUMENT_NAME).click();
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.findByTestId("dropdown-panel")
      .contains("Log an Under/Over redaction")
      .click();
    cy.findByTestId("div-modal").should("have.length", 1);
    cy.findByTestId("rl-under-over-redaction-content").should("be.visible");
    cy.findByTestId("btn-redaction-log-cancel").click();

    //verify that under redaction log feature is available
    cy.selectPDFTextElement("Multi Media Evidence");
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.findByTestId("select-redaction-type").should("have.length", 1);
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").should("not.be.disabled");
  });
});

export { };
