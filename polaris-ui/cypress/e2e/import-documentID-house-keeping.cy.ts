describe("case details page", () => {
  it("accepts an additional parameter passed in the URL", () => {
    // add an existing mock documentId - 12
    cy.visit("/case-details/16KR1506126/2195986/12");
    cy.url({ decode: true }).should("includes", "/12");
  });

  xit("should given document ID via URL open successfully in a tab", () => {
    // add an existing mock documentId - 12
    cy.visit("/case-details/16KR1506126/2195986/12");
    const pdfContainer = cy.findByTestId("div-pdfviewer-0");
    pdfContainer.should("exist");
  });
});
