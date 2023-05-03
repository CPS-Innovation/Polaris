import { SAVE_REDACTION_ROUTE, FILE_ROUTE } from "../../../src/mock-api/routes";

describe("redaction assurance", () => {
  it("redaction assurance", () => {
    const fileNameWithoutExtension = `1`;

    cy.overrideRoute(
      SAVE_REDACTION_ROUTE,
      { type: "writeRequest", fileName: fileNameWithoutExtension },
      "put"
    );
    //cy.viewport(1500, 2500);
    cy.visit("/case-details/12AB1111111/13401");

    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0").should("exist").contains("ABC");

    cy.get("body").first().screenshot(`${fileNameWithoutExtension}-pre`, {
      overwrite: true,
      capture: "fullPage",
    });

    // cy.findByTestId("div-pdfviewer-1")
    //   .realMouseDown({ position: { x: 207, y: 130 } })
    //   .realMouseUp({ position: { x: 525, y: 365 } });

    cy.get(".page")
      .last()
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
