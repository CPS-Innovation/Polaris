import { SAVE_REDACTION_ROUTE, FILE_ROUTE } from "../../../src/mock-api/routes";
import pdfStrings from "../../assurance-input/pdf-strings.json";
import { _base64ToArrayBuffer } from "../../../src/mock-api/handlers";
import assuranceInput from "../../assurance-input/assuranceInput.json";
const getFileBase64 = (blobName: string) => {
  const fileBase64 = (pdfStrings as { [key: string]: string })[blobName!];
  return fileBase64;
};

describe("redaction assurance", () => {
  it("should redact pdfs and export the coordinates file", () => {
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();

    assuranceInput.forEach((input) => {
      cy.overrideRoute(
        SAVE_REDACTION_ROUTE,
        { type: "writeRequest", fileName: input.fileName },
        "put"
      );

      cy.overrideRoute(FILE_ROUTE, {
        type: "returnFile",
        body: _base64ToArrayBuffer(getFileBase64(input.fileName)),
      });
      //cy.viewport(1500, 2500);

      // cy.findByTestId("div-pdfviewer-0").should("exist").contains("1");
      cy.wait(2000);

      input.coordinates.forEach((coordinate) => {
        cy.get(`[data-page-number=${coordinate.pageIndex}]`).as(
          `page-${coordinate.pageIndex}`
        );
        cy.get(`@page-${coordinate.pageIndex}`).scrollIntoView();
        cy.get(`@page-${coordinate.pageIndex}`).screenshot(
          `${input.fileName}-page${coordinate.pageIndex}-pre`,
          {
            overwrite: true,
            capture: "fullPage",
          }
        );
        cy.get(`@page-${coordinate.pageIndex}`)
          .realMouseDown({ position: { x: coordinate.x0, y: coordinate.y0 } })
          .realMouseUp({ position: { x: coordinate.x1, y: coordinate.y1 } });

        cy.findByTestId("btn-redact").click();

        cy.get(`@page-${coordinate.pageIndex}`).screenshot(
          `${input.fileName}-page${coordinate.pageIndex}-post`,
          {
            overwrite: true,
            capture: "fullPage",
          }
        );
      });

      cy.findByTestId("btn-save-redaction-0").click();

      cy.window()
        .its("latestWriteRequest")
        .then((reqBody) => {
          cy.writeFile(
            `cypress/assurance-output/${input.fileName}.json`,
            JSON.stringify(JSON.parse(reqBody!.toString()), null, 2)
          );
        });
    });
  });

  //old test case
  // it("redaction assurance", () => {
  //   const fileNameWithoutExtension = `1`;

  //   cy.overrideRoute(
  //     SAVE_REDACTION_ROUTE,
  //     { type: "writeRequest", fileName: fileNameWithoutExtension },
  //     "put"
  //   );
  //   //cy.viewport(1500, 2500);
  //   cy.visit("/case-details/12AB1111111/13401");

  //   cy.findByTestId("btn-accordion-open-close-all").click();
  //   cy.findByTestId("link-document-1").click();
  //   cy.findByTestId("div-pdfviewer-0").should("exist").contains("1");

  //   cy.get("body").first().screenshot(`${fileNameWithoutExtension}-pre`, {
  //     overwrite: true,
  //     capture: "fullPage",
  //   });

  //   // cy.findByTestId("div-pdfviewer-1")
  //   //   .realMouseDown({ position: { x: 207, y: 130 } })
  //   //   .realMouseUp({ position: { x: 525, y: 365 } });

  //   cy.get(".page")
  //     .first()
  //     .realMouseDown({ position: { x: 60, y: 60 } })
  //     .realMouseUp({ position: { x: 285, y: 190 } });
  //   cy.findByTestId("btn-redact").click({ force: true });

  //   cy.get(".page")
  //     .last()
  //     .realMouseDown({ position: { x: 60, y: 50 } })
  //     .realMouseUp({ position: { x: 285, y: 180 } });

  //   cy.findByTestId("btn-redact").click({ force: true });

  //   cy.get("body").first().screenshot(`${fileNameWithoutExtension}-post`, {
  //     overwrite: true,
  //     capture: "fullPage",
  //   });

  //   cy.findByTestId("btn-save-redaction-0").click();
  // });
});
