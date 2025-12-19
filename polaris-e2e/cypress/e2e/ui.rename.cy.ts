const {
  RENAME_DOCUMENT_URN,
  RENAME_DOCUMENT_CASE_ID,
  RENAME_DOCUMENT_DOCUMENT_ID,
} = Cypress.env();

const EXPECTED_DOCUMENT_NAME = "e2e-test-document-name-renamed";

describe("Rename refresh flow", { tags: ["@ci", "@ci-chunk-2"] }, () => {
  xit("can add a note and refresh the tracker", () => {
    cy.fullLogin();
    cy.clearCaseTracker(RENAME_DOCUMENT_URN, RENAME_DOCUMENT_CASE_ID);

    cy.visit("/polaris-ui/case-search");
    cy.setPolarisInstrumentationGuid("PHASE_1");

    cy.findByTestId("input-search-urn").type(`${RENAME_DOCUMENT_URN}{enter}`);
    cy.findByTestId(`link-${RENAME_DOCUMENT_URN}`).click();

    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId(`link-document-CMS-${RENAME_DOCUMENT_DOCUMENT_ID}`).should(
      "not.contain",
      EXPECTED_DOCUMENT_NAME
    );

    cy.findAllByTestId(
      `document-housekeeping-actions-dropdown-CMS-${RENAME_DOCUMENT_DOCUMENT_ID}`
    ).click();

    cy.setPolarisInstrumentationGuid("PHASE_2");

    cy.findByText("Rename document").click();
    cy.findByTestId("rename-text-input").clear();
    cy.findByTestId("rename-text-input").type(EXPECTED_DOCUMENT_NAME);
    cy.findByTestId("btn-save-rename").click();
    cy.findByTestId("btn-close-rename").click();

    cy.findByTestId(`link-document-CMS-${RENAME_DOCUMENT_DOCUMENT_ID}`).should(
      "contain",
      EXPECTED_DOCUMENT_NAME
    );
  });
});

export {};
