describe("case details page", () => {
  it("accepts an additional document parameter in the URL and opens the referenced document", () => {
    cy.visit("/case-details/45CV2911222/13401/12");

    cy.location("pathname", { timeout: 20000 }).should(
      "include",
      "/case-details/45CV2911222/13401/12"
    );
    cy.findByTestId("div-pdfviewer-0", { timeout: 20000 }).should("exist");
  });
});
