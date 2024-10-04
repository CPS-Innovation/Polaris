import { redactionRequestAssertionValidator } from "../utils/redactionAssuranceUtils";

describe("Redaction Assurance", () => {
  const expectedSaveRedactionPayload = {
    documentId: "1",
    redactions: [
      {
        pageIndex: 1,
        height: 1415.65,
        width: 1001,
        redactionCoordinates: [
          { x1: 402.94, y1: 1391, x2: 620.98, y2: 1371.5 },
          { x1: 168.27, y1: 1118.2, x2: 239.16, y2: 1098.7 },
          { x1: 390.43, y1: 1085.31, x2: 426.77, y2: 1065.81 },
          { x1: 206.81, y1: 337.74, x2: 264.60, y2: 318.24 },
        ],
      },
      {
        pageIndex: 3,
        height: 1415.65,
        width: 1001,
        redactionCoordinates: [
          { x1: 56.7, y1: 749.97, x2: 653.07, y2: 730.47 },
          { x1: 56.7, y1: 564.13, x2: 733.14, y2: 544.63 },
        ],
      },
    ],
    documentModifications: [],
  };
  describe("Document Fullscreen", () => {
    it("Should successfully verify the save redaction request data in non-full screen mode", () => {
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.findByTestId("btn-save-redaction-0").click();

      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in non full screen and full screen mode", () => {
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("full-screen-btn").click();
      cy.wait(1000);
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();

      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });
  });

  describe("Screen Resize", () => {
    it("Should successfully verify the save redaction request data in given screen size(1300, 1000)", () => {
      cy.viewport(1300, 1000);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(1200,1000)", () => {
      cy.viewport(1200, 1000);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(1100, 1000)", () => {
      cy.viewport(1100, 1000);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(1000,1000)", () => {
      cy.viewport(1000, 1000);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(900,1000)", () => {
      cy.viewport(900, 1000);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(700,1000)", () => {
      cy.viewport(700, 1000);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(800,900)", () => {
      cy.viewport(800, 900);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(700,700)", () => {
      cy.viewport(700, 700);
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
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("MCLOVE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("Male");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("BYRNE");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("FRESH SWELLING");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("medical treatment");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedSaveRedactionPayload,
          JSON.parse(saveRequestObject.body)
        );
      });
    });
  });

  describe("Mixed Orientation PDf (Portrait and Landscape)", () => {
    const expectedMixedOrientationPDfSaveRequest = {
      documentId: "10",
      redactions: [
        {
          pageIndex: 1,
          height: 1347.18,
          width: 1041,
          redactionCoordinates: [
            { x1: 70.16, y1: 1111.72, x2: 90.97, y2: 1089.72 },
            { x1: 896.84, y1: 1111.72, x2: 928.04, y2: 1089.72 },
            { x1: 70.16, y1: 797.61, x2: 99.99, y2: 775.61 },
            { x1: 896.84, y1: 797.61, x2: 928.04, y2: 775.61 },
            { x1: 70.16, y1: 458.78, x2: 101.37, y2: 436.78 },
            { x1: 896.84, y1: 458.78, x2: 928.04, y2: 436.78 },
            { x1: 70.16, y1: 144.68, x2: 101.37, y2: 122.68 },
            { x1: 896.84, y1: 144.68, x2: 928.04, y2: 122.68 },
          ],
        },
        {
          pageIndex: 2,
          height: 1041,
          width: 1347.18,
          redactionCoordinates: [
            { x1: 70.16, y1: 780.8, x2: 90.97, y2: 758.8 },
            { x1: 1172.4, y1: 780.8, x2: 1203.6, y2: 758.8 },
            { x1: 70.16, y1: 590.41, x2: 99.99, y2: 568.41 },
            { x1: 1172.4, y1: 590.41, x2: 1203.6, y2: 568.41 },
            { x1: 70.16, y1: 375.28, x2: 101.37, y2: 353.28 },
            { x1: 1172.4, y1: 375.28, x2: 1203.6, y2: 353.28 },
            { x1: 70.16, y1: 160.16, x2: 101.37, y2: 138.16 },
            { x1: 1172.4, y1: 160.16, x2: 1203.6, y2: 138.16 },
          ],
        },
      ],
      documentModifications: [],
    };
    it("Should successfully verify the save redaction request data in given screen size(1300, 1000)", () => {
      cy.viewport(1300, 1000);
      const saveRequestObject = { body: "" };
      cy.trackRequestBody(
        saveRequestObject,
        "PUT",
        "/api/urns/12AB1111111/cases/13401/documents/10"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("Page1 Portrait");
      cy.selectPDFTextElement("p1");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p10");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p11");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p20");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p21");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p30");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p31");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p40");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("L1");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L10");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L11");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L20");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L21");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L30");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L31");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L40");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedMixedOrientationPDfSaveRequest,
          JSON.parse(saveRequestObject.body)
        );
      });
    });

    it("Should successfully verify the save redaction request data in given screen size(1000, 1000)", () => {
      cy.viewport(1000, 1000);
      const saveRequestObject = { body: "" };
      cy.trackRequestBody(
        saveRequestObject,
        "PUT",
        "/api/urns/12AB1111111/cases/13401/documents/10"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-10").click();
      cy.findByTestId("div-pdfviewer-0")
        .should("exist")
        .contains("Page1 Portrait");
      cy.selectPDFTextElement("p1");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p10");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p11");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p20");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p21");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p30");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p31");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("p40");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.selectPDFTextElement("L1");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L10");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L11");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L20");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L21");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L30");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L31");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();
      cy.selectPDFTextElement("L40");
      cy.findByTestId("btn-redact").should("be.disabled");
      cy.findByTestId("select-redaction-type").should("have.length", 1);
      cy.findByTestId("select-redaction-type").select("2");
      cy.findByTestId("btn-redact").click();

      cy.findByTestId("btn-save-redaction-0").click();
      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        redactionRequestAssertionValidator(
          expectedMixedOrientationPDfSaveRequest,
          JSON.parse(saveRequestObject.body)
        );
      });
    });
  });
});
