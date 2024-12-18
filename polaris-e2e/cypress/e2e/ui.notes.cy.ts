const {
  REFRESH_NOTE_CASE_URN,
  REFRESH_NOTE_CASE_ID,
  REFRESH_NOTE_DOCUMENT_ID,
  REFRESH_NOTE_TARGET_TEXT,
} = Cypress.env();

describe("Notes refresh flow", { tags: ["@ci", "@ci-chunk-2"] }, () => {
  it("can add a note and refresh the tracker", () => {
    cy.fullLogin();
    cy.clearCaseTracker(REFRESH_NOTE_CASE_URN, REFRESH_NOTE_CASE_ID);

    cy.visit(
      `/polaris-ui/case-details/${REFRESH_NOTE_CASE_URN}/${REFRESH_NOTE_CASE_ID}?notes=true`
    );
    cy.setPolarisInstrumentationGuid("PHASE_1");

    cy.findByTestId("btn-accordion-open-close-all").click();

    // tracker will have completed phase 1 stuff so switch now to phase 2
    cy.setPolarisInstrumentationGuid("PHASE_2");

    // assert that the blue-blob has notes indicator is not present
    cy.findByTestId(
      `has-note-indicator-CMS-${REFRESH_NOTE_DOCUMENT_ID}`
    ).should("not.exist");
    cy.findByTestId(`btn-notes-CMS-${REFRESH_NOTE_DOCUMENT_ID}`).click();

    cy.findByTestId("notes-textarea").type("e2e-test-note");
    cy.findByTestId("btn-add-note").click();
    cy.findByTestId("btn-cancel-notes").click();

    // assert that the blue-blob has notes indicator is now there
    cy.findByTestId(
      `has-note-indicator-CMS-${REFRESH_NOTE_DOCUMENT_ID}`
    ).should("exist");

    cy.findByTestId(`btn-notes-CMS-${REFRESH_NOTE_DOCUMENT_ID}`).click();
    cy.findAllByText(REFRESH_NOTE_TARGET_TEXT);
  });
});

export {};
