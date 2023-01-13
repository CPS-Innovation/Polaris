/// <reference types="cypress" />

const {
    USERNAME,
    TARGET_URN,
    TARGET_DEFENDANT_NAME,
    TARGET_DOCUMENT_ID,
    TARGET_DOCUMENT_NAME,
    TARGET_DOCUMENT_TEXT_FRAGMENT,
    TARGET_SEARCH_TEXT,
} = Cypress.env();

describe("Polaris", () => {
    it("can view a case", () => {
        // have we logged in OK?
        cy.login().contains(USERNAME);

        // search for our URN
        cy.findByTestId("input-search-urn").type(`${TARGET_URN}{enter}`);

        // open case details page
        cy.findByTestId(`link-${Cypress.env().TARGET_URN}`).click();

        // is our defendant correct
        cy.findByTestId("txt-defendant-name").contains(TARGET_DEFENDANT_NAME);

        // open our target document
        cy.findByTestId("btn-accordion-open-close-all").click();
        cy.findByTestId(`link-document-${TARGET_DOCUMENT_ID}`).click();

        cy.findByTestId("btn-open-pdf").contains(TARGET_DOCUMENT_NAME);
        cy.get("span").contains(TARGET_DOCUMENT_TEXT_FRAGMENT);

        cy.findByTestId("input-search-case").type(
            `${TARGET_SEARCH_TEXT}{enter}`
        );
        cy.findByTestId(`link-result-document-${TARGET_DOCUMENT_ID}`).click();

        cy.findByTestId("tabs").contains(
            `1 match for "${TARGET_SEARCH_TEXT}" in ${TARGET_DOCUMENT_NAME}`
        );
    });
});
