/// <reference types="cypress" />

const {
  WITNESS_TARGET_URN,
  WITNESS_TARGET_CASE_ID,
  WITNESS_DOCUMENT_ID,
  WITNESS_EXPECTED_INDICATORS,
  WITNESS_NOT_EXPECTED_INDICATORS,
} = Cypress.env();

describe("Witness Indicators", { tags: ["@ci", "@ci-chunk-3"] }, () => {
  xit("can display witness indicators", () => {
    cy.on("uncaught:exception", () => false);

    cy.fullLogin();
    cy.clearCaseTracker(WITNESS_TARGET_URN, WITNESS_TARGET_CASE_ID);

    cy.visit("/polaris-ui");
    cy.findByTestId("input-search-urn").type(`${WITNESS_TARGET_URN}{enter}`);
    cy.findByTestId(`link-${WITNESS_TARGET_URN}`).click();

    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId(`link-document-${WITNESS_DOCUMENT_ID}`);

    cy.get(`[data-testid^="indicator-${WITNESS_DOCUMENT_ID}-"]`)
      .should("have.length", 10)
      .then(($indicators) => {
        const elements: HTMLElement[] = Array.from($indicators);

        const codes = elements
          .map((el) => {
            const testid =
              el.dataset?.testid !== undefined ? el.dataset.testid : null;

            if (!testid) {
              throw new Error("Missing data-testid on indicator element");
            }
            const parts = testid.split("-");
            const code = parts[3];
            if (!code) {
              throw new Error(`Cannot parse indicator code from '${testid}'`);
            }
            return code;
          })
          .join(",");

        expect(codes).to.equal(WITNESS_EXPECTED_INDICATORS);
      });

    (WITNESS_NOT_EXPECTED_INDICATORS as string)
      .split(",")
      .forEach((indicator) => {
        cy.findByTestId(`indicator-${WITNESS_DOCUMENT_ID}-${indicator}`).should(
          "not.exist"
        );
      });
  });
});
