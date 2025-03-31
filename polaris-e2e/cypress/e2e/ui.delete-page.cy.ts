/// <reference types="cypress" />

const {
  DELETE_PAGE_TARGET_URN,
  DELETE_PAGE_CASE_ID,
  DELETE_PAGE_DOCUMENT_ID,
} = Cypress.env();

describe("Delete Page", { tags: ["@ci", "@ci-chunk-1"] }, () => {
  it("can view a case", () => {
    cy.on("uncaught:exception", () => false);

    cy.fullLogin();

    cy.clearCaseTracker(DELETE_PAGE_TARGET_URN, DELETE_PAGE_CASE_ID);
    cy.visit("/polaris-ui");

    cy.findByTestId("input-search-urn").type(`${DELETE_PAGE_TARGET_URN}{enter}`);

    // open case details page
    cy.findByTestId(`link-${DELETE_PAGE_TARGET_URN}`).click();

    // open our target document
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId(`link-document-${DELETE_PAGE_DOCUMENT_ID}`).should(
      "contain",
      "Timothy"
    ).click();

    cy.wait(3000);

    // Delete page
    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("delete-page-modal").should("exist");
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("btn-save-redaction-0").click();
    cy.findByTestId("div-modal").should("be.visible");
    cy.findByTestId("rl-saving-redactions").should(
      "have.text",
      "Saving redactions..."
    );
    cy.findByTestId("rl-saved-redactions").should(
      "have.text",
      "Redactions successfully saved"
    );

  });
});

export { };
