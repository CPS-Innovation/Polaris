import { SAVE_REDACTION_ROUTE } from "../../../src/mock-api/routes";
// import pdfs from "../../../src/mock-api/data/pdfs/pdf-strings.json";

describe("redaction assurance", () => {
  it("redaction assurance", () => {
    const fileNameWithoutExtension = `1`;

    cy.overrideRoute(
      SAVE_REDACTION_ROUTE,
      { type: "writeRequest", fileName: fileNameWithoutExtension },
      "put"
    );

    cy.visit("/case-details/12AB1111111/13401");

    // const documentRoute = `${Cypress.env(
    //   "REACT_APP_GATEWAY_BASE_URL"
    // )}/api/urns/12AB1111111/cases/13401/documents/1?v=1`;

    // cy.log(documentRoute);

    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0").should("exist").contains("ABC");

    cy.get("body").first().screenshot(`${fileNameWithoutExtension}-pre`, {
      overwrite: true,
      capture: "fullPage",
    });

    cy.findByTestId("div-pdfviewer-0")
      .realMouseDown({ position: { x: 207, y: 130 } })
      .realMouseUp({ position: { x: 525, y: 365 } });

    cy.findByTestId("btn-redact").click({ force: true });

    cy.get("body").first().screenshot(`${fileNameWithoutExtension}-post`, {
      overwrite: true,
      capture: "fullPage",
    });

    cy.findByTestId("btn-save-redaction-0").click();
  });
});
