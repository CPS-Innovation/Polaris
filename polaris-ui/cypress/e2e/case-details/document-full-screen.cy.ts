import { RedactionSaveRequest } from "../../../src/app/features/cases/domain/gateway/RedactionSaveRequest";

/**
 * Validator function to compare the redaction values with a precision factor
 * @param expectedRequest
 * @param request
 */
const redactionRequestAssertionValidator = (
  expectedRequest: RedactionSaveRequest,
  request: RedactionSaveRequest
) => {
  const PRECISION_FACTOR = 0.3;
  expect(request.documentId).to.equal(expectedRequest.documentId);
  expect(request.redactions.length).to.equal(expectedRequest.redactions.length);
  request.redactions.forEach((redaction, index) => {
    expect(redaction.height).to.equal(expectedRequest.redactions[index].height);
    expect(redaction.pageIndex).to.equal(
      expectedRequest.redactions[index].pageIndex
    );
    expect(
      Math.abs(redaction.width - expectedRequest.redactions[index].width)
    ).to.be.lessThan(PRECISION_FACTOR);

    const coordinates = redaction.redactionCoordinates;
    const expectedCoordinates =
      expectedRequest.redactions[index].redactionCoordinates;
    coordinates.forEach((coordinate, index) => {
      expect(
        Math.abs(coordinate.x1 - expectedCoordinates[index].x1)
      ).to.be.lessThan(PRECISION_FACTOR);
      expect(
        Math.abs(coordinate.y1 - expectedCoordinates[index].y1)
      ).to.be.lessThan(PRECISION_FACTOR);
      expect(
        Math.abs(coordinate.x2 - expectedCoordinates[index].x2)
      ).to.be.lessThan(PRECISION_FACTOR);
      expect(
        Math.abs(coordinate.y2 - expectedCoordinates[index].y2)
      ).to.be.lessThan(PRECISION_FACTOR);
    });
  });
};

describe("Document Fullscreen", () => {
  const expectedSaveRedactionPayload = {
    documentId: "1",
    redactions: [
      {
        pageIndex: 1,
        height: 1472.21,
        width: 1041,
        redactionCoordinates: [
          { x1: 419.04, y1: 1446.53, x2: 650.79, y2: 1426.53 },
          { x1: 174.99, y1: 1162.82, x2: 253.73, y2: 1142.82 },
          { x1: 406.04, y1: 1128.62, x2: 448.88, y2: 1108.62 },
          { x1: 215.08, y1: 351.18, x2: 280.17, y2: 331.18 },
        ],
      },
      {
        pageIndex: 3,
        height: 1472.21,
        width: 1041,
        redactionCoordinates: [
          { x1: 58.96, y1: 779.87, x2: 689.19, y2: 759.87 },
          { x1: 58.96, y1: 586.61, x2: 767.79, y2: 566.61 },
        ],
      },
    ],
  };
  it(
    "Should successfully verify the save redaction request data in non-full screen mode",
    {
      viewportHeight: 1000,
      viewportWidth: 1500,
    },
    () => {
      const saveRequestObject = { body: "" };
      cy.trackRequestBody(
        saveRequestObject,
        "PUT",
        "/api/urns/12AB1111111/cases/13401/documents/1"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").click();

      cy.findByTestId("btn-save-redaction-0").click();

      //assertion on the redaction log save request
      cy.window().then(() => {
        expect(JSON.stringify(expectedSaveRedactionPayload)).to.deep.equal(
          saveRequestObject.body
        );
      });
    }
  );

  it(
    "Should successfully verify the save redaction request data in full screen mode",
    {
      viewportHeight: 1000,
      viewportWidth: 1500,
    },
    () => {
      const saveRequestObject = { body: "" };
      cy.trackRequestBody(
        saveRequestObject,
        "PUT",
        "/api/urns/12AB1111111/cases/13401/documents/1"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-1").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
      cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("full-screen-btn").click();
      cy.wait(1000);
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();

      //assertion on the redaction log save request
      cy.window().then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    }
  );

  it("Should show the 'Show Full Screen' button when at least one document is opened in a tab", () => {
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("full-screen-btn").should("not.exist");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.findByTestId("full-screen-btn").should("exist");
  });

  it("Should hide the 'Show Full Screen' button when no documents are opened in a tab", () => {
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("full-screen-btn").should("not.exist");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.findByTestId("full-screen-btn").should("exist");
    cy.findByTestId("tab-remove").click();
    cy.findByTestId("full-screen-btn").should("not.exist");
  });

  it("Should show and hide the document in full screen on clicking the full screen btn and should show correct tooltip", () => {
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.findByTestId("side-panel").should("exist");
    cy.findByTestId("full-screen-btn").trigger("mouseover");
    cy.findByTestId("tooltip").should("contain", "View full screen");
    cy.findByTestId("full-screen-btn").click();
    cy.findByTestId("side-panel").should("not.exist");
    cy.findByTestId("full-screen-btn").trigger("mouseover");
    cy.findByTestId("tooltip").should("contain", "Exit full screen");
    cy.findByTestId("full-screen-btn").click();
    cy.findByTestId("side-panel").should("exist");
  });

  it("Should not show the 'Show Full Screen' button if the full screen feature is turned off", () => {
    cy.visit("/case-details/12AB1111111/13401?fullScreen=false");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.findByTestId("side-panel").should("exist");
    cy.findByTestId("full-screen-btn").should("not.exist");
  });
});