import { SAVE_ROTATION_ROUTE } from "../../src/mock-api/routes";
describe("Feature Rotate Page", () => {
  it("Should be able to turn off/on the rotate page feature", () => {
    cy.visit("/case-details/12AB1111111/13401?pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Show Rotate Page Options").click();
    cy.get("div.page").then((pages) => {
      cy.wrap(pages).each((pageDiv) => {
        cy.wait(100);
        const pageNumber = pageDiv.attr("data-page-number");
        cy.waitUntil(() => {
          return cy.wrap(pageDiv).scrollIntoView();
        }).then(() => {
          cy.wrap(pageDiv)
            .findByTestId(`page-number-text-${pageNumber}`)
            .should("exist");

          cy.wrap(pageDiv)
            .findByTestId(`btn-rotate-${pageNumber}`)
            .should("exist");

          cy.wrap(pageDiv)
            .findByTestId(`btn-rotate-cancel-${pageNumber}`)
            .should("not.exist");
        });
      });
    });
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Hide Rotate Page Options").click();
    cy.get("div.page").then((pages) => {
      cy.wrap(pages).each((pageDiv) => {
        cy.wait(100);
        const pageNumber = pageDiv.attr("data-page-number");
        cy.waitUntil(() => {
          return cy.wrap(pageDiv).scrollIntoView();
        }).then(() => {
          cy.wrap(pageDiv)
            .findByTestId(`page-number-text-${pageNumber}`)
            .should("not.exist");

          cy.wrap(pageDiv)
            .findByTestId(`btn-rotate-${pageNumber}`)
            .should("not.exist");

          cy.wrap(pageDiv)
            .findByTestId(`btn-rotate-cancel-${pageNumber}`)
            .should("not.exist");
        });
      });
    });
  });

  it("Should show page rotate button and page number correctly in each page", () => {
    cy.visit("/case-details/12AB1111111/13401?pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Show Rotate Page Options").click();
    cy.get("div.page").then((pages) => {
      const totalPages = pages.length;
      cy.wrap(pages).each((pageDiv) => {
        cy.wait(100);
        const pageNumber = pageDiv.attr("data-page-number");

        cy.waitUntil(() => {
          return cy.wrap(pageDiv).scrollIntoView();
        }).then(() => {
          cy.wrap(pageDiv)
            .findByTestId(`page-number-text-${pageNumber}`)
            .should("contain.text", `Page:${pageNumber}/${totalPages}`);
          cy.wrap(pageDiv)
            .findByTestId(`btn-rotate-${pageNumber}`)
            .should("have.text", "Rotate page");
          cy.wrap(pageDiv)
            .findByTestId(`btn-rotate-cancel-${pageNumber}`)
            .should("not.exist");
        });
      });
    });
  });

  it("Should successfully complete page rotation", () => {
    const expectedSaveRequest = {
      documentModifications: [
        { pageIndex: 1, operation: "rotate", arg: "90" },
        { pageIndex: 2, operation: "rotate", arg: "-90" },
        { pageIndex: 3, operation: "rotate", arg: "-180" },
      ],
    };
    const saveRequestObject = { body: "" };
    cy.trackRequestBody(
      saveRequestObject,
      "POST",
      "/api/urns/12AB1111111/cases/13401/documents/1/modify"
    );
    cy.visit("/case-details/12AB1111111/13401?pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.wait(1000);
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Show Rotate Page Options").click();
    cy.findByTestId("btn-rotate-1").should("exist");
    cy.findByTestId("btn-cancel-rotate-1").should("not.exist");
    cy.findByTestId("btn-rotate-1").click();
    cy.findByTestId("btn-cancel-rotate-1").should("exist");
    cy.findByTestId("rotate-page-overlay-1").should("exist");
    cy.findByTestId("rotate-page-left-btn-1").should("exist");
    cy.findByTestId("rotate-page-right-btn-1").should("exist");
    cy.findByTestId("rotation-overlay-cancel-btn").should("exist");
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 0°");
    cy.findByTestId("rotate-page-content-1").should(
      "not.contain",
      `Click "Save all rotations" to submit changes to CMS`
    );
    cy.findByTestId("rotation-count-text-0").should("not.exist");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 90°");
    cy.findByTestId("rotate-page-content-1").contains(
      `Click "Save all rotations" to submit changes to CMS`
    );
    cy.findByTestId("rotation-count-text-0").should(
      "have.text",
      "There is 1 rotation"
    );
    cy.findByTestId("rotation-overlay-cancel-btn").click();
    cy.findByTestId("rotate-page-overlay-1").should("not.exist");
    cy.findByTestId("rotate-page-content-1").should("not.exist");
    cy.findByTestId("rotation-count-text-0").should("not.exist");

    cy.findByTestId("btn-rotate-1").click();
    cy.findByTestId("btn-rotate-1").should("not.exist");
    cy.findByTestId("btn-cancel-rotate-1").should("exist");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 90°");
    cy.findByTestId("btn-rotate-2").click();
    cy.findByTestId("btn-rotate-2").should("not.exist");
    cy.findByTestId("btn-cancel-rotate-2").should("exist");
    cy.findByTestId("rotate-page-right-btn-2").click();
    cy.findByTestId("rotate-page-content-2").contains("Rotate page 90°");
    cy.findByTestId("rotation-count-text-0").should(
      "have.text",
      "There are 2 rotations"
    );
    cy.findByTestId("btn-link-removeAll-0").click();
    cy.findByTestId("btn-rotate-1").should("exist");
    cy.findByTestId("btn-cancel-rotate-1").should("not.exist");
    cy.findByTestId("btn-rotate-2").should("exist");
    cy.findByTestId("btn-cancel-rotate-2").should("not.exist");
    cy.findByTestId("rotate-page-content-1").should("not.exist");
    cy.findByTestId("rotate-page-content-2").should("not.exist");
    cy.findByTestId("rotation-count-text-0").should("not.exist");
    cy.findByTestId("btn-link-removeAll-0").should("not.exist");

    cy.findByTestId("btn-rotate-1").click();
    cy.findByTestId("btn-cancel-rotate-1").should("exist");
    cy.findByTestId("rotate-page-overlay-1").should("exist");
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 0°");
    cy.findByTestId("btn-cancel-rotate-1").click();
    cy.findByTestId("rotate-page-overlay-1").should("not.exist");
    cy.findByTestId("rotate-page-content-1").should("not.exist");
    cy.findByTestId("rotation-count-text-0").should("not.exist");
    cy.findByTestId("btn-rotate-1").click();
    cy.findByTestId("btn-cancel-rotate-1").should("exist");
    cy.findByTestId("rotate-page-overlay-1").should("exist");
    cy.findByTestId("rotation-count-text-0").should("not.exist");
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 0°");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 90°");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotation-count-text-0").should("exist");
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 180°");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 270°");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 0°");
    cy.findByTestId("rotation-count-text-0").should("not.exist");
    cy.findByTestId("rotate-page-left-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page -90°");
    cy.findByTestId("rotation-count-text-0").should("exist");
    cy.findByTestId("rotate-page-left-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page -180°");
    cy.findByTestId("rotate-page-left-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page -270°");
    cy.findByTestId("rotate-page-left-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 0°");
    cy.findByTestId("rotation-count-text-0").should("not.exist");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 90°");
    cy.findByTestId("rotation-count-text-0").should(
      "have.text",
      "There is 1 rotation"
    );
    cy.findByTestId("btn-rotate-2").click();
    cy.findByTestId("btn-cancel-rotate-2").should("exist");
    cy.findByTestId("rotate-page-overlay-2").should("exist");
    cy.findByTestId("rotate-page-content-2").contains("Rotate page 0°");
    cy.findByTestId("rotate-page-left-btn-2").click();
    cy.findByTestId("rotate-page-content-2").contains("Rotate page -90°");
    cy.findByTestId("rotation-count-text-0").should(
      "have.text",
      "There are 2 rotations"
    );

    cy.findByTestId("btn-rotate-3").click();
    cy.findByTestId("btn-cancel-rotate-3").should("exist");
    cy.findByTestId("rotate-page-overlay-3").should("exist");
    cy.findByTestId("rotate-page-content-3").contains("Rotate page 0°");
    cy.findByTestId("rotate-page-left-btn-3").click();
    cy.findByTestId("rotate-page-content-3").contains("Rotate page -90°");
    cy.findByTestId("rotate-page-left-btn-3").click();
    cy.findByTestId("rotate-page-content-3").contains("Rotate page -180°");
    cy.findByTestId("rotation-count-text-0").should(
      "have.text",
      "There are 3 rotations"
    );

    cy.findByTestId("btn-rotate-4").click();
    cy.findByTestId("btn-cancel-rotate-4").should("exist");
    cy.findByTestId("rotate-page-overlay-4").should("exist");
    cy.findByTestId("rotate-page-content-4").contains("Rotate page 0°");
    cy.findByTestId("rotation-count-text-0").should(
      "have.text",
      "There are 3 rotations"
    );
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("btn-save-rotations-0").click();
    cy.findByTestId("div-modal").should("exist");

    cy.waitUntil(() => {
      return saveRequestObject.body;
    }).then(() => {
      expect(saveRequestObject.body).to.deep.equal(
        JSON.stringify(expectedSaveRequest)
      );
    });

    cy.findByTestId("div-modal").contains(
      "Document updated successfully saved to CMS"
    );
    cy.findByTestId("btn-modal-close").click();
    cy.findByTestId("div-modal").should("not.exist");

    //document checkout and cancel checkout tracking
  });

  it("Should show error message when failed to save the rotation and should enable back the save rotation button", () => {
    cy.overrideRoute(
      SAVE_ROTATION_ROUTE,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 3000,
      },
      "post"
    );
    cy.visit("/case-details/12AB1111111/13401?pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.wait(1000);
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Show Rotate Page Options").click();
    cy.findByTestId("btn-rotate-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 0°");
    cy.findByTestId("rotate-page-right-btn-1").click();
    cy.findByTestId("rotate-page-content-1").contains("Rotate page 90°");

    cy.findByTestId("btn-save-rotations-0").click();
    cy.findByTestId("btn-save-rotations-0").should("be.disabled");
    cy.findByTestId("btn-link-removeAll-0").should("be.disabled");
    cy.findByTestId("div-modal").contains("Saving updated document to CMS...");

    cy.findByTestId("div-modal").contains("h1", "Something went wrong!");
    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to save rotations. Please try again.");
    cy.findByTestId("div-modal").contains(
      "If re-trying is not successful, please notify the Casework App product team."
    );

    cy.findByTestId("btn-error-modal-ok").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.findByTestId("btn-save-rotations-0").should("not.be.disabled");
    cy.findByTestId("btn-link-removeAll-0").should("not.be.disabled");
  });

  it("Should show redaction warning when user tries to do redaction when page rotation mode is on", () => {
    cy.visit("/case-details/12AB1111111/13401?pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.wait(1000);
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Show Rotate Page Options").click();
    cy.findByTestId("redaction-warning").should("not.exist");
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("not.exist");
    cy.findByTestId("redaction-warning").should("exist");
    cy.findByTestId("redaction-warning").contains(
      "Redaction is unavailable in page rotation mode, please turn off page rotation to continue with redaction."
    );
  });

  it("Should show warning message when the user tries to turn on rotate page option, if there are unsaved redactions", () => {
    cy.visit("/case-details/12AB1111111/13401?pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.wait(1000);
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Show Rotate Page Options").click();

    cy.findByTestId("div-modal").contains("h2", "Save your redactions");
    cy.findByTestId("div-modal")
      .should("exist")
      .contains(
        "You cannot rotate pages as you have unsaved redactions and these will be lost."
      );
    cy.findByTestId("div-modal").contains(
      "Remove or save your redactions and you will be able to continue."
    );
    cy.findByTestId("btn-modal-close").click();
    cy.findByTestId("div-modal").should("not.exist");
  });
});
