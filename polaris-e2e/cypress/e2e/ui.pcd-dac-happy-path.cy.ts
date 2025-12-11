/// <reference types="cypress" />

const { PCD_DAC_URN, PCD_DAC_CASE_ID, PCD_DOCUMENT_ID } = Cypress.env();

describe("PCD and DAC documents", { tags: ["@ci", "@ci-chunk-1"] }, () => {
  xit("can view a DAC document", () => {
    cy.fullLogin();

    cy.clearCaseTracker(PCD_DAC_URN, PCD_DAC_CASE_ID);
    cy.visit("/polaris-ui");

    cy.findByTestId("input-search-urn").type(`${PCD_DAC_URN}{enter}`);

    // open case details page
    cy.findByTestId(`link-${PCD_DAC_URN}(M)`).click();

    // check we have 2 defendants and open the document
    cy.findByTestId("link-defendant-details").contains("2").click();

    // find some text in the dac and check that we cannot redact dac documents
    cy.selectPDFTextElement("TENTACLES");
    cy.findByTestId("redaction-warning");

    // open our target document
    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId(`link-document-${PCD_DOCUMENT_ID}`).click();

    // find some text in the pcd and check that we cannot redact pcd documents
    cy.selectPDFTextElement("H6400");
    cy.findByTestId("redaction-warning");
  });
});

export {};
