import { SAVE_REDACTION_ROUTE } from "../../../src/mock-api/routes";

describe("redaction assurance", () => {
  it("For Single defendant and single charge, should show defendant details, charge details and custody time limits and Youth Offender if applicable", () => {
    cy.visit("/case-details/12AB1111111/13401");
    cy.writeFile("request.txt", "Hello, world");
    const fileNameWithoutExtension = `redaction-${+new Date()}`;

    cy.overrideRoute(SAVE_REDACTION_ROUTE, { type: "writeRequest" }, "put");

    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    //cy.selectPDFTextElement("9");

    cy.findByTestId("div-pdfviewer-0")
      .realMouseDown({ position: { x: 10, y: 10 } })
      .realMouseUp({ position: { x: 200, y: 200 } });

    cy.findByTestId("btn-redact").click({ force: true });

    // .trigger("mouseover")
    // .trigger("mousedown", { which: 1 })
    // .trigger("mousemove", {
    //   clientX: 100,
    //   clientY: 100,
    //   screenX: 100,
    //   screenY: 100,
    //   pageX: 100,
    //   pageY: 100,
    // })
    // .trigger("mouseup", { which: 1 });

    // cy.findByTestId("btn-redact").click({ force: true });
    // cy.screenshot(`${fileNameWithoutExtension}.png`);
    // cy.findByTestId("btn-save-redaction-0").click();
  });
});
