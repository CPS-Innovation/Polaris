import { redactionRequestAssertionValidator } from "../utils/redactionAssuranceUtils";
describe("Feature Delete Page", () => {
  it("Should show page delete button and page number correctly in each page", () => {
    cy.visit("/case-details/12AB1111111/13401?pageDelete=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.get("div.page").then((pages) => {
      const totalPages = pages.length;
      cy.wrap(pages).each((pageDiv) => {
        const pageNumber = pageDiv.attr("data-page-number");
        cy.wrap(pageDiv)
          .findByTestId(`delete-page-number-text-${pageNumber}`)
          .should("contain.text", `Page:${pageNumber}/${totalPages}`);
        cy.wrap(pageDiv)
          .findByTestId(`btn-delete-${pageNumber}`)
          .should("have.text", `Delete`);
        cy.wrap(pageDiv)
          .findByTestId(`btn-cancel-delete-${pageNumber}`)
          .should("not.exist");
      });
    });
  });

  it("Should be able to turn off/on the page delete feature", () => {
    cy.visit("/case-details/12AB1111111/13401?pageDelete=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Hide Delete Page Options").click();
    cy.get("div.page").then((pages) => {
      cy.wrap(pages).each((pageDiv) => {
        const pageNumber = pageDiv.attr("data-page-number");
        cy.wrap(pageDiv)
          .findByTestId(`delete-page-number-text-${pageNumber}`)
          .should("not.exist");

        cy.wrap(pageDiv)
          .findByTestId(`btn-delete-${pageNumber}`)
          .should("not.exist");

        cy.wrap(pageDiv)
          .findByTestId(`btn-cancel-delete-${pageNumber}`)
          .should("not.exist");
      });
    });
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Show Delete Page Options").click();
    cy.get("div.page").then((pages) => {
      cy.wrap(pages).each((pageDiv) => {
        const pageNumber = pageDiv.attr("data-page-number");
        cy.wrap(pageDiv)
          .findByTestId(`delete-page-number-text-${pageNumber}`)
          .should("exist");

        cy.wrap(pageDiv)
          .findByTestId(`btn-delete-${pageNumber}`)
          .should("exist");

        cy.wrap(pageDiv)
          .findByTestId(`btn-cancel-delete-${pageNumber}`)
          .should("not.exist");
      });
    });
  });

  it("Should successfully complete deleting pages with other redaction", () => {
    const expectedSaveRequest = {
      documentId: "1",
      redactions: [
        {
          pageIndex: 3,
          height: 1415.65,
          width: 1001,
          redactionCoordinates: [
            { x1: 56.33, y1: 1038.7, x2: 245.83, y2: 1015.52 },
          ],
        },
      ],
      documentModifications: [
        { pageIndex: 1, operation: "delete" as const },
        { pageIndex: 2, operation: "delete" as const },
      ],
    };
    const saveRequestObject = { body: "" };
    cy.trackRequestBody(
      saveRequestObject,
      "PUT",
      "/api/urns/12AB1111111/cases/13401/documents/1"
    );
    const expectedRedactionLogRequest = {
      urn: "99ZZ9999999",
      unit: {
        id: "9-1",
        type: "Area",
        areaDivisionName: "South East",
        name: "Magistrates Court",
      },
      investigatingAgency: { id: "43", name: "Greater Manchester Police" },
      documentType: { id: "35", name: "Other" },
      redactions: [
        {
          missedRedaction: { id: "2", name: "Title" },
          redactionType: 1,
          returnedToInvestigativeAuthority: false,
        },
        {
          missedRedaction: {
            id: "16",
            name: "MG11 Backsheet",
            isDeletedPage: true,
          },
          redactionType: 1,
          returnedToInvestigativeAuthority: false,
        },
        {
          missedRedaction: {
            id: "16",
            name: "MG11 Backsheet",
            isDeletedPage: true,
          },
          redactionType: 1,
          returnedToInvestigativeAuthority: false,
        },
      ],
      notes: null,
      chargeStatus: 2,
      cmsValues: {
        originalFileName: "MCLOVEMG3.pdf",
        documentId: 1,
        documentType: "MG11",
        fileCreatedDate: "2020-06-01",
        documentTypeId: 1,
      },
    };
    const redactionLogSaveRequestObject = { body: "" };
    cy.trackRequestBody(
      redactionLogSaveRequestObject,
      "POST",
      "/api/redactionLogs"
    );
    cy.visit("/case-details/12AB1111111/13401?pageDelete=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.wait(1000);
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId("btn-cancel-delete-1").should("not.exist");
    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("delete-page-modal").should("exist");
    cy.findByTestId("delete-page-modal-btn-redact").should("be.disabled");
    cy.findByTestId("delete-page-modal-btn-cancel").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId("btn-delete-1").click();

    cy.findByTestId("select-redaction-type")
      .find("option")
      .then((options) => {
        const optionTexts = Array.from(options).map(
          (option) => option.textContent
        );
        expect(optionTexts).to.deep.equal([
          "-- Please select --",
          "MG11 Backsheet",
          "Contains personal data",
          "Blank page",
        ]);
      });

    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId("btn-delete-1").should("not.exist");
    cy.findByTestId("btn-cancel-delete-1").should("exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`delete-page-content-1`).contains(
      `Click "save all redactions" to remove the page from the document`
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );
    cy.findByTestId("btn-cancel-delete-1").click();
    cy.findByTestId(`redaction-count-text-0`).should("not.exist");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId(`delete-page-content-1`).should("not.exist");

    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );

    cy.findByTestId("btn-delete-2").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-2`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-2`).should("exist");
    cy.findByTestId(`delete-page-content-2`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There are 2 redactions"
    );
    cy.selectPDFTextElement("CIRCUMSTANCES:");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There are 3 redactions"
    );
    cy.findByTestId("btn-save-redaction-0").click();
    cy.waitUntil(() => {
      return saveRequestObject.body;
    }).then(() => {
      redactionRequestAssertionValidator(
        expectedSaveRequest,
        JSON.parse(saveRequestObject.body)
      );
    });

    cy.findByTestId("div-modal").should("have.length", 1);

    cy.get("h1").contains("99ZZ9999999 - Redaction Log").should("exist");
    cy.get("h2").contains('Redaction details for:"MCLOVEMG3"').should("exist");
    cy.findByTestId("rl-under-redaction-content").should("have.length", 1);
    cy.findByTestId("redaction-summary")
      .get("li:nth-child(1)")
      .should("contain", "2 - MG11 Backsheets");
    cy.findByTestId("redaction-summary")
      .get("li:nth-child(2)")
      .should("contain", "1 - Title");
    cy.findByTestId("btn-save-redaction-log").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.waitUntil(() => {
      return redactionLogSaveRequestObject;
    }).then(() => {
      expect(redactionLogSaveRequestObject.body).to.deep.equal(
        JSON.stringify(expectedRedactionLogRequest)
      );
    });
  });

  it("should remove any unsaved redactions in the page selected for deletion", () => {
    const expectedSaveRequest = {
      documentId: "1",
      redactions: [],
      documentModifications: [{ pageIndex: 1, operation: "delete" as const }],
    };
    const saveRequestObject = { body: "" };
    cy.trackRequestBody(
      saveRequestObject,
      "PUT",
      "/api/urns/12AB1111111/cases/13401/documents/1"
    );
    cy.visit("/case-details/12AB1111111/13401?pageDelete=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.wait(1000);
    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );
    cy.selectPDFTextElement("MCLOVE");
    cy.findByTestId("btn-redact").should("have.length", 1);
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.focused().should("have.id", "select-redaction-type");
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click({ force: true });
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There are 2 redactions"
    );
    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );
    cy.findByTestId("btn-save-redaction-0").click();
    cy.waitUntil(() => {
      return saveRequestObject.body;
    }).then(() => {
      redactionRequestAssertionValidator(
        expectedSaveRequest,
        JSON.parse(saveRequestObject.body)
      );
    });
  });

  it("should keep any unsaved redactions page deletions even if we switch the tab", () => {
    cy.visit("/case-details/12AB1111111/13401?pageDelete=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-1").click();
    cy.wait(1000);
    cy.findByTestId("div-pdfviewer-0")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");
    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );

    cy.findByTestId("btn-delete-2").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-2`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-2`).should("exist");
    cy.findByTestId(`delete-page-content-2`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There are 2 redactions"
    );
    cy.findByTestId("link-document-10").click();
    cy.wait(1000);
    cy.findByTestId("div-pdfviewer-1")
      .should("exist")
      .contains("Page1 Portrait");
    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-1`).should(
      "have.text",
      "There is 1 redaction"
    );
    cy.findByTestId("btn-tab-0").click();

    cy.findByTestId("btn-cancel-delete-1").should("exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId("btn-cancel-delete-2").should("exist");
    cy.findByTestId(`delete-page-overlay-2`).should("exist");
    cy.findByTestId(`delete-page-content-2`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There are 2 redactions"
    );
    cy.findByTestId("btn-tab-1").click();
    cy.findByTestId("btn-cancel-delete-1").should("exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-1`).should(
      "have.text",
      "There is 1 redaction"
    );
  });

  it("should not be able to delete last page of available page of a document for deletion", () => {
    cy.visit("/case-details/12AB1111111/13401?pageDelete=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-10").click();
    cy.wait(1000);
    cy.findByTestId("btn-delete-1").should("not.be.disabled");
    cy.findByTestId("btn-delete-2").should("not.be.disabled");
    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );

    cy.findByTestId("btn-delete-2").should("be.disabled");
    cy.findByTestId("btn-cancel-delete-1").click();

    cy.findByTestId("btn-delete-2").should("not.be.disabled");
    cy.findByTestId("btn-delete-1").should("not.be.disabled");
    cy.findByTestId("btn-delete-2").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-2`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId("delete-page-modal").should("not.exist");
    cy.findByTestId(`delete-page-overlay-2`).should("exist");
    cy.findByTestId(`delete-page-content-2`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );
    cy.findByTestId("btn-delete-1").should("be.disabled");
  });

  it("should ignore and show warning message if a user try to hide deletion option or show rotate page option, when there is an unsaved page deletion", () => {
    cy.visit("/case-details/12AB1111111/13401?pageDelete=true&pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-10").click();
    cy.wait(1000);
    cy.findByTestId("btn-delete-1").should("not.be.disabled");
    cy.findByTestId("btn-delete-1").click();
    cy.findByTestId("select-redaction-type").select("MG11 Backsheet");
    cy.findByTestId("delete-page-modal-btn-redact").should("not.be.disabled");
    cy.findByTestId(`delete-page-overlay-1`).should("not.exist");
    cy.findByTestId("delete-page-modal-btn-redact").click();
    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Hide Delete Page Options").click();
    cy.findByTestId("div-modal").contains("h2", "Save your redactions");
    cy.findByTestId("div-modal")
      .should("exist")
      .contains(
        "You cannot turn off deletion feature as you have unsaved redactions and these will be lost."
      );
    cy.findByTestId("div-modal").contains(
      "Remove or save your redactions and you will be able to continue."
    );
    cy.findByTestId("btn-modal-close").click();
    cy.findByTestId("div-modal").should("not.exist");

    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
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

    cy.findByTestId(`delete-page-overlay-1`).should("exist");
    cy.findByTestId(`delete-page-content-1`).contains(
      "Page selected for deletion"
    );
    cy.findByTestId(`redaction-count-text-0`).should(
      "have.text",
      "There is 1 redaction"
    );
  });

  it("should not show delete page feature for PCD document", () => {
    cy.visit("/case-details/12AB1111111/13401?pageRotate=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("link-document-13").click();
    cy.findByTestId("document-actions-dropdown-0").click();
    cy.contains("button", "Hide Delete Page Options").should("not.exist");
    cy.findByTestId(`delete-page-number-text-1`).should("not.exist");
    cy.findByTestId(`btn-delete-1`).should("not.exist");
  });
});
