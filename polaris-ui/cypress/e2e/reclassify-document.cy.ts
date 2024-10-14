import {
  SAVE_RECLASSIFY,
  MATERIAL_TYPE_LIST,
  EXHIBIT_PRODUCERS,
  STATEMENT_WITNESS,
  STATEMENT_WITNESS_NUMBERS,
  TRACKER_ROUTE,
} from "../../src/mock-api/routes";
import { refreshPipelineReclassifyDocuments } from "../../src/mock-api/data/pipelinePdfResults.cypress";

describe("Feature Reclassify Document", () => {
  it("Should show reclassify document option if the document 'canReclassify' is true and should not show if it is not ", () => {
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click(); //12;
    cy.findByTestId("document-housekeeping-actions-dropdown-1").should("exist");
    cy.findByTestId("dropdown-panel").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document");
    cy.realPress("Escape");
    cy.findByTestId("dropdown-panel").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-2").should("exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-2").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document");
    cy.realPress("Escape");
    cy.findByTestId("document-housekeeping-actions-dropdown-3").should(
      "not.exist"
    );
    cy.findByTestId("document-housekeeping-actions-dropdown-4").click();
    cy.findByTestId("dropdown-panel")
      .contains("Reclassify document")
      .should("not.exist");
  });

  it("Should show the re-classification content and be able to close reclassification if the user click back or cancel button and but back the focus correctly", () => {
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .contains("button", "Back", { timeout: 10000 })
      .should("be.visible")
      .click();
    cy.findByTestId("div-reclassify").should("not.exist");

    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .contains("button", "Cancel", { timeout: 10000 })
      .should("be.visible")
      .click();
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
  });

  it("should navigate to the reclassification page and load all the document type list ", () => {
    const getMaterialListCounter = { count: 0 };
    cy.trackRequestCount(
      getMaterialListCounter,
      "GET",
      "/api/reference/reclassification"
    );
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();

    cy.waitUntil(() => {
      return getMaterialListCounter.count;
    }).then(() => {
      expect(getMaterialListCounter.count).to.equal(1);
    });
    cy.findByTestId("reclassify-document-type")
      .find("option:selected")
      .should("have.text", "Choose document type");
    cy.findByTestId("reclassify-document-type")
      .find("option")
      .then((options) => {
        const optionTexts = Array.from(options).map(
          (option) => option.textContent
        );
        expect(optionTexts).to.deep.equal([
          "Choose document type",
          "MG10",
          "MG11",
          "MG15(SDN)",
          "Other Communication",
          "MG12",
        ]);
      });
  });

  it("should successful complete the document classification to an `Immediate` type ", () => {
    const getMaterialListCounter = { count: 0 };
    cy.trackRequestCount(
      getMaterialListCounter,
      "GET",
      "/api/reference/reclassification"
    );
    const trackerResults = refreshPipelineReclassifyDocuments("10", 1015, 2);
    cy.overrideRoute(TRACKER_ROUTE, {
      body: trackerResults[0],
    });
    const expectedSaveReclassifyPayload = {
      documentId: 10,
      documentTypeId: 1015,
      immediate: { documentName: null },
      other: null,
      statement: null,
      exhibit: null,
    };
    const saveReclassifyRequestObject = { body: "" };
    cy.trackRequestBody(
      saveReclassifyRequestObject,
      "POST",
      "/api/urns/12AB1111111/cases/13401/documents/10/reclassify"
    );
    const refreshPipelineCounter = { count: 0 };
    cy.trackRequestCount(
      refreshPipelineCounter,
      "POST",
      "/api/urns/12AB1111111/cases/13401"
    );

    const trackerCounter = { count: 0 };
    cy.trackRequestCount(
      trackerCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/tracker"
    );
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");

    cy.findByTestId("reclassify-document-type").then((select) => {
      const selectId = select.attr("id");
      cy.get(`label[for="${selectId}"]`).should(
        "have.text",
        "Select the document type for PortraitLandscape"
      );
    });

    cy.findByTestId("reclassify-document-type")
      .find("option")
      .should("have.length", 6);
    cy.findByTestId("reclassify-document-type").select("MG10");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the document details");
    cy.findByTestId("div-reclassify")
      .find("legend")
      .should("have.length", 1)
      .and(
        "have.text",
        "Do you want to change the document name of PortraitLandscape?"
      );
    cy.get('input[type="radio"][name="change-document-name"][value="YES"]')
      .should("exist")
      .should("not.be.checked")
      .then((radioYes) => {
        const Id = radioYes.attr("id");
        cy.get(`label[for="${Id}"]`).should("have.text", "Yes");
      });
    cy.get('input[type="radio"][name="change-document-name"][value="NO"]')
      .should("exist")
      .should("be.checked")
      .then((radioNo) => {
        const Id = radioNo.attr("id");
        cy.get(`label[for="${Id}"]`).should("have.text", "No");
      });
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("div-reclassify")
      .find("h2")
      .should("have.length", 1)
      .and("have.text", "Document details");
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .should("have.length", 2);
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(0)
      .find("td")
      .then(($cells) => {
        expect($cells.eq(0)).to.have.text("Type");
        expect($cells.eq(1)).to.have.text("MG10");
        expect($cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(1)
      .find("td")
      .then(($cells) => {
        expect($cells.eq(0)).to.have.text("Name");
        expect($cells.eq(1)).to.have.text("PortraitLandscape");
        expect($cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("div-notification-banner").should("not.exist");
    cy.findByTestId("reclassify-save-btn").click();
    cy.findByTestId("div-notification-banner").should("exist");
    cy.findByTestId("div-notification-banner").contains(
      "Saving to CMS. Please wait."
    );
    cy.waitUntil(() => {
      return saveReclassifyRequestObject.body;
    }).then(() => {
      expect(saveReclassifyRequestObject.body).to.deep.equal(
        JSON.stringify(expectedSaveReclassifyPayload)
      );
      cy.overrideRoute(TRACKER_ROUTE, {
        body: trackerResults[1],
      });
    });

    cy.waitUntil(() => {
      return trackerCounter.count === 2;
    }).then(() => {
      expect(trackerCounter.count).to.equal(2);
      expect(refreshPipelineCounter.count).to.equal(2);
    });
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
  });

  it("should successful complete the document classification to an `Other` type ", () => {
    const getMaterialListCounter = { count: 0 };
    cy.trackRequestCount(
      getMaterialListCounter,
      "GET",
      "/api/reference/reclassification"
    );
    const trackerResults = refreshPipelineReclassifyDocuments("10", 1029, 2);
    cy.overrideRoute(TRACKER_ROUTE, {
      body: trackerResults[0],
    });
    const expectedSaveReclassifyPayload = {
      documentId: 10,
      documentTypeId: 1029,
      immediate: null,
      other: { documentName: null, used: true },
      statement: null,
      exhibit: null,
    };
    const saveReclassifyRequestObject = { body: "" };
    cy.trackRequestBody(
      saveReclassifyRequestObject,
      "POST",
      "/api/urns/12AB1111111/cases/13401/documents/10/reclassify"
    );
    const refreshPipelineCounter = { count: 0 };
    cy.trackRequestCount(
      refreshPipelineCounter,
      "POST",
      "/api/urns/12AB1111111/cases/13401"
    );

    const trackerCounter = { count: 0 };
    cy.trackRequestCount(
      trackerCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/tracker"
    );
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");

    cy.findByTestId("reclassify-document-type").then((select) => {
      const selectId = select.attr("id");
      cy.get(`label[for="${selectId}"]`).should(
        "have.text",
        "Select the document type for PortraitLandscape"
      );
    });

    cy.findByTestId("reclassify-document-type")
      .find("option")
      .should("have.length", 6);
    cy.findByTestId("reclassify-document-type").select("Other Communication");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the document details");
    cy.findByTestId("div-reclassify").find("legend").should("have.length", 2);
    cy.findByTestId("div-reclassify")
      .find("legend")
      .eq(0)
      .should(
        "have.text",
        "Do you want to change the document name of PortraitLandscape?"
      )
      .next()
      .within(() => {
        cy.get('input[type="radio"][name="change-document-name"][value="YES"]')
          .should("exist")
          .should("not.be.checked")
          .then((radioYes) => {
            const Id = radioYes.attr("id");
            cy.get(`label[for="${Id}"]`).should("have.text", "Yes");
          });
        cy.get('input[type="radio"][name="change-document-name"][value="NO"]')
          .should("exist")
          .should("be.checked")
          .then((radioNo) => {
            const Id = radioNo.attr("id");
            cy.get(`label[for="${Id}"]`).should("have.text", "No");
          });
      });

    cy.findByTestId("div-reclassify")
      .find("legend")
      .eq(1)
      .should("have.text", "What is the document status?")
      .next()
      .within(() => {
        cy.get(
          'input[type="radio"][name="radio-document-used-status"][value="YES"]'
        )
          .should("exist")
          .should("be.checked")
          .then((radioUsed) => {
            const Id = radioUsed.attr("id");
            cy.get(`label[for="${Id}"]`).should("have.text", "Used");
          });
        cy.get(
          'input[type="radio"][name="radio-document-used-status"][value="NO"]'
        )
          .should("exist")
          .should("not.be.checked")
          .then((radioUnused) => {
            const Id = radioUnused.attr("id");
            cy.get(`label[for="${Id}"]`).should("have.text", "Unused");
          });
      });
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("div-reclassify")
      .find("h2")
      .should("have.length", 1)
      .and("have.text", "Document details");
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .should("have.length", 3);
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(0)
      .find("td")
      .then(($cells) => {
        expect($cells.eq(0)).to.have.text("Type");
        expect($cells.eq(1)).to.have.text("Other Communication");
        expect($cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(1)
      .find("td")
      .then(($cells) => {
        expect($cells.eq(0)).to.have.text("Name");
        expect($cells.eq(1)).to.have.text("PortraitLandscape");
        expect($cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(2)
      .find("td")
      .then(($cells) => {
        expect($cells.eq(0)).to.have.text("Status");
        expect($cells.eq(1)).to.have.text("Used");
        expect($cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("div-notification-banner").should("not.exist");
    cy.findByTestId("reclassify-save-btn").click();
    cy.findByTestId("div-notification-banner").should("exist");
    cy.findByTestId("div-notification-banner").contains(
      "Saving to CMS. Please wait."
    );

    cy.waitUntil(() => {
      return saveReclassifyRequestObject.body;
    }).then(() => {
      expect(saveReclassifyRequestObject.body).to.deep.equal(
        JSON.stringify(expectedSaveReclassifyPayload)
      );
      cy.overrideRoute(TRACKER_ROUTE, {
        body: trackerResults[1],
      });
    });

    cy.waitUntil(() => {
      return trackerCounter.count === 2;
    }).then(() => {
      expect(trackerCounter.count).to.equal(2);
      expect(refreshPipelineCounter.count).to.equal(2);
    });
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-10");
  });
});
