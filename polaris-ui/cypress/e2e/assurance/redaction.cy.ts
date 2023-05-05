import { SAVE_REDACTION_ROUTE, FILE_ROUTE } from "../../../src/mock-api/routes";
import pdfStrings from "../../assurance-input/pdf-strings.json";
import { _base64ToArrayBuffer } from "../../../src/mock-api/handlers";
import assuranceInput from "../../assurance-input/assuranceInput.json";
const getFileBase64 = (blobName: string) => {
  const fileBase64 = (pdfStrings as { [key: string]: string })[blobName!];
  return fileBase64;
};

const compareNumberValuesWithTolerance = (
  val1: number,
  val2: number,
  tolerance: number = 1
) => {
  return Math.abs(val1 - val2) <= tolerance;
};

const redactionsAsserts = (currentRedactions: any, expectedRedactions: any) => {
  currentRedactions.forEach((current: any, index: number) => {
    const expectedCurrent = expectedRedactions[index];
    Object.keys(current).forEach((key) => {
      if (typeof current[key] === "number") {
        expect(
          compareNumberValuesWithTolerance(current[key], expectedCurrent[key])
        ).to.equal(true);
      }
      if (Array.isArray(current[key])) {
        current[key].forEach((coordinates: any, subIndex: number) => {
          const expectedCoordinates = expectedCurrent[key][subIndex];
          Object.keys(coordinates).forEach((subKey) => {
            expect(
              compareNumberValuesWithTolerance(
                coordinates[subKey],
                expectedCoordinates[subKey]
              )
            ).to.equal(true);
          });
        });
      }
    });
  });
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
      cy.viewport(1500, 1000);

      cy.wait(2000);

      input.coordinates.forEach((coordinate) => {
        cy.get(`[data-page-number=${coordinate.pageIndex}]`).as(
          `page-${coordinate.pageIndex}`
        );
        cy.get(`@page-${coordinate.pageIndex}`).scrollIntoView();
        // cy.get(`@page-${coordinate.pageIndex}`).screenshot(
        //   `${input.fileName}-page${coordinate.pageIndex}-pre`,
        //   {
        //     overwrite: true,
        //     capture: "fullPage",
        //   }
        // );

        cy.get(`@page-${coordinate.pageIndex}`)
          .realMouseDown({ position: { x: coordinate.x0, y: coordinate.y0 } })
          .realMouseUp({ position: { x: coordinate.x1, y: coordinate.y1 } });

        cy.findByTestId("btn-redact").click({ force: true });

        // cy.get(`@page-${coordinate.pageIndex}`).screenshot(
        //   `${input.fileName}-page${coordinate.pageIndex}-post`,
        //   {
        //     overwrite: true,
        //     capture: "fullPage",
        //   }
        // );
      });

      cy.findByTestId("btn-save-redaction-0").click();

      cy.window()
        .its("latestWriteRequest")
        .then((reqBody) => {
          cy.writeFile(
            `cypress/assurance-output/${input.fileName}.json`,
            JSON.stringify(JSON.parse(reqBody!.toString()), null, 2)
          );
          console.log(
            "JSON.parse(reqBody!.toString()",
            JSON.parse(reqBody!.toString())
          );

          const currentRedactions = JSON.parse(reqBody!.toString()).redactions;
          redactionsAsserts(currentRedactions, input.redactions);
        });
    });
  });
});
