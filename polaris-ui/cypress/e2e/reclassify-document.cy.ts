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
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Type");
        expect(cells.eq(1)).to.have.text("MG10");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(1)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Name");
        expect(cells.eq(1)).to.have.text("PortraitLandscape");
        expect(cells.eq(2)).to.have.text("Change");
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
      expect(getMaterialListCounter.count).to.equal(1);
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
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Type");
        expect(cells.eq(1)).to.have.text("Other Communication");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(1)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Name");
        expect(cells.eq(1)).to.have.text("PortraitLandscape");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(2)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Status");
        expect(cells.eq(1)).to.have.text("Used");
        expect(cells.eq(2)).to.have.text("Change");
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
      expect(getMaterialListCounter.count).to.equal(1);
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

  it("should successful complete the document classification to a `Statement` type", () => {
    const getMaterialListCounter = { count: 0 };
    cy.trackRequestCount(
      getMaterialListCounter,
      "GET",
      "/api/reference/reclassification"
    );
    const getWitnessCounter = { count: 0 };
    cy.trackRequestCount(
      getWitnessCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/witnesses"
    );
    const getWitnessStatementCounter = { count: 0 };
    cy.trackRequestCount(
      getWitnessStatementCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/witnesses/1/statements"
    );
    const trackerResults = refreshPipelineReclassifyDocuments("10", 1031, 2);
    cy.overrideRoute(TRACKER_ROUTE, {
      body: trackerResults[0],
    });
    const expectedSaveReclassifyPayload = {
      documentId: 10,
      documentTypeId: 1031,
      immediate: null,
      other: null,
      statement: {
        used: true,
        witnessId: 1,
        statementNo: 5,
        date: "1980-10-01",
      },
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
    cy.findByTestId("reclassify-document-type").select("MG11");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the statement details");
    cy.findByTestId("reclassify-statement-witness")
      .find("option:selected")
      .should("have.text", "Select a Witness");
    cy.findByTestId("reclassify-statement-witness")
      .find("option")
      .then((options) => {
        const optionTexts = Array.from(options).map(
          (option) => option.textContent
        );
        expect(optionTexts).to.deep.equal([
          "Select a Witness",
          "PC Blaynee_S",
          "PC Jones_S",
          "PC Lucy_S",
        ]);
      });
    cy.findByTestId("reclassify-statement-witness").select("PC Blaynee_S");

    cy.findByTestId("div-reclassify").find("legend").should("have.length", 2);
    cy.findByTestId("div-reclassify")
      .find("legend")
      .eq(0)
      .should("have.text", "Statement date")
      .next()
      .should("have.text", "For example, 27 3 2024")
      .next()
      .within(() => {
        cy.get('input[type="text"][name="statement-date-day"]')
          .should("exist")
          .type("01")
          .then((textDay) => {
            const Id = textDay.attr("id");
            cy.get(`label[for="${Id}"]`).should("have.text", "Day");
          });
        cy.get('input[type="text"][name="statement-date-month"]')
          .should("exist")
          .type("10")
          .then((textMonth) => {
            const Id = textMonth.attr("id");
            cy.get(`label[for="${Id}"]`).should("have.text", "Month");
          });
        cy.get('input[type="text"][name="statement-date-year"]')
          .should("exist")
          .type("1980")
          .then((textMonth) => {
            const Id = textMonth.attr("id");
            cy.get(`label[for="${Id}"]`).should("have.text", "Year");
          });
      });

    cy.findAllByTestId("reclassify-statement-number")
      .should("have.value", "")
      .then((textStatementNumber) => {
        const Id = textStatementNumber.attr("id");
        cy.get(`label[for="${Id}"]`)
          .should("have.text", "Statement Number")
          .next()
          .should("have.text", "Already in use #2 - #4, #7");
      });
    cy.findAllByTestId("reclassify-statement-number").type("5");

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
      .should("have.length", 5);
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(0)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Type");
        expect(cells.eq(1)).to.have.text("MG11");
        expect(cells.eq(2)).to.have.text("Change");
      });

    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(1)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Statement Witness");
        expect(cells.eq(1)).to.have.text("PC Blaynee_S");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(2)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Statement Date");
        expect(cells.eq(1)).to.have.text("01/10/1980");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(3)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Statement Number");
        expect(cells.eq(1)).to.have.text("5");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(4)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Status");
        expect(cells.eq(1)).to.have.text("Used");
        expect(cells.eq(2)).to.have.text("Change");
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
      expect(getMaterialListCounter.count).to.equal(1);
      expect(getWitnessCounter.count).to.equal(1);
      expect(getWitnessStatementCounter.count).to.equal(1);
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

  it("should successful complete the document classification to a `Exhibit` type", () => {
    const getMaterialListCounter = { count: 0 };
    cy.trackRequestCount(
      getMaterialListCounter,
      "GET",
      "/api/reference/reclassification"
    );
    const getExhibitProducersCounter = { count: 0 };
    cy.trackRequestCount(
      getExhibitProducersCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/exhibit-producers"
    );

    const trackerResults = refreshPipelineReclassifyDocuments("1", 1042, 2);
    cy.overrideRoute(TRACKER_ROUTE, {
      body: trackerResults[0],
    });
    const expectedSaveReclassifyPayload = {
      documentId: 1,
      documentTypeId: 1042,
      immediate: null,
      other: null,
      statement: null,
      exhibit: {
        used: true,
        existingProducerOrWitnessId: 1,
        newProducer: null,
        item: "MCLOVEMG3_1",
        reference: "test reference",
      },
    };
    const saveReclassifyRequestObject = { body: "" };
    cy.trackRequestBody(
      saveReclassifyRequestObject,
      "POST",
      "/api/urns/12AB1111111/cases/13401/documents/1/reclassify"
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
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");

    cy.findByTestId("reclassify-document-type").then((select) => {
      const selectId = select.attr("id");
      cy.get(`label[for="${selectId}"]`).should(
        "have.text",
        "Select the document type for MCLOVEMG3"
      );
    });

    cy.findByTestId("reclassify-document-type")
      .find("option")
      .should("have.length", 6);
    cy.findByTestId("reclassify-document-type").select("MG15(SDN)");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the exhibit details");
    cy.findByTestId("div-reclassify").should(
      "contain",
      "You're entering exhibit details for MCLOVEMG3"
    );

    cy.findByTestId("exhibit-item-name")
      .should("have.value", "MCLOVEMG3")
      .then((item) => {
        const Id = item.attr("id");
        cy.get(`label[for="${Id}"]`).should("have.text", "Item Name");
      });
    cy.findByTestId("exhibit-item-name").clear().type("MCLOVEMG3_1");

    cy.findByTestId("exhibit-reference")
      .should("have.value", "")
      .then((reference) => {
        const Id = reference.attr("id");
        cy.get(`label[for="${Id}"]`).should("have.text", "Exhibit Reference");
      });
    cy.findByTestId("exhibit-reference").type("test reference");
    cy.findByTestId("exhibit-select-producer")
      .find("option:selected")
      .should("have.text", "Select a Producer");
    cy.findByTestId("exhibit-select-producer")
      .find("option")
      .then((options) => {
        const optionTexts = Array.from(options).map(
          (option) => option.textContent
        );
        expect(optionTexts).to.deep.equal([
          "Select a Producer",
          "PC Blaynee",
          "PC Jones",
          "PC Lucy",
          "Other producer or witness",
        ]);
      });
    cy.findByTestId("exhibit-select-producer").select("PC Blaynee");

    cy.findByTestId("div-reclassify")
      .find("legend")
      .eq(0)
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
      .should("have.length", 5);
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(0)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Type");
        expect(cells.eq(1)).to.have.text("MG15(SDN)");
        expect(cells.eq(2)).to.have.text("Change");
      });

    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(1)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Item Name");
        expect(cells.eq(1)).to.have.text("MCLOVEMG3_1");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(2)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Exhibit Reference");
        expect(cells.eq(1)).to.have.text("test reference");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(3)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Exhibit Producer");
        expect(cells.eq(1)).to.have.text("PC Blaynee");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(4)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Status");
        expect(cells.eq(1)).to.have.text("Used");
        expect(cells.eq(2)).to.have.text("Change");
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
      expect(getMaterialListCounter.count).to.equal(1);
      expect(getExhibitProducersCounter.count).to.equal(1);
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
    cy.focused().should("have.id", "document-housekeeping-actions-dropdown-1");
  });

  it("should show error message if save reclassify is unsuccessful and shouldn't call refreshPipeline and tracker", () => {
    cy.overrideRoute(
      SAVE_RECLASSIFY,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "post"
    );
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
    cy.findByTestId("reclassify-document-type").select("MG10");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the document details");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("div-notification-banner").should("not.exist");
    cy.findByTestId("reclassify-save-btn").click();
    cy.findByTestId("div-notification-banner").should("exist");
    cy.findByTestId("div-notification-banner").contains(
      "Saving to CMS. Please wait."
    );
    cy.waitUntil(() => {
      return saveReclassifyRequestObject.body;
    }).then(() => {
      expect(getMaterialListCounter.count).to.equal(1);
      expect(saveReclassifyRequestObject.body).to.deep.equal(
        JSON.stringify(expectedSaveReclassifyPayload)
      );
      cy.overrideRoute(TRACKER_ROUTE, {
        body: trackerResults[1],
      });
    });

    cy.findByTestId("div-modal")
      .should("exist")
      .contains("Failed to save reclassification. Please try again later.");
    cy.findByTestId("btn-error-modal-ok").click();
    cy.findByTestId("div-modal").should("not.exist");
    cy.window().then(() => {
      expect(refreshPipelineCounter.count).to.equal(1);
      expect(trackerCounter.count).to.equal(1);
    });
  });

  it("Should show all the reclassify UI validation errors", () => {
    const saveReclassifyRequestObject = { body: "" };
    cy.trackRequestBody(
      saveReclassifyRequestObject,
      "POST",
      "/api/urns/12AB1111111/cases/13401/documents/1/reclassify"
    );
    const expectedSaveReclassifyPayload = {
      documentId: 1,
      documentTypeId: 1042,
      immediate: null,
      other: null,
      statement: null,
      exhibit: {
        used: false,
        existingProducerOrWitnessId: null,
        newProducer: "producer",
        item: "abc",
        reference: "test_ref",
      },
    };
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("div-reclassify").should("not.exist");
    cy.findByTestId("document-housekeeping-actions-dropdown-10").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");
    cy.findByTestId("reclassify-continue-btn").click();

    //stage 1 validation
    cy.findByTestId("reclassify-doctypeId-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-document-type-link").should(
      "have.text",
      "New document type should not be empty"
    );
    cy.findByTestId("reclassify-document-type-link").click();
    cy.focused().should("have.id", "reclassify-document-type");
    cy.get("#reclassify-document-type-error").should(
      "have.text",
      "Error: New document type should not be empty"
    );

    //Immediate and Others type validation
    cy.findByTestId("reclassify-document-type").select("MG10");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the document details");
    cy.get(
      'input[type="radio"][name="change-document-name"][value="YES"]'
    ).check();
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-document-new-name-link").should(
      "have.text",
      "New name should be different from current name"
    );
    cy.findByTestId("reclassify-document-new-name-link").click();
    cy.focused().should("have.id", "document-new-name");
    cy.get("#document-new-name-error").should(
      "have.text",
      "Error: New name should be different from current name"
    );
    cy.findByTestId("document-new-name").clear();
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-document-new-name-link").should(
      "have.text",
      "New name should not be empty"
    );
    cy.findByTestId("reclassify-document-new-name-link").click();
    cy.focused().should("have.id", "document-new-name");
    cy.get("#document-new-name-error").should(
      "have.text",
      "Error: New name should not be empty"
    );
    const maxLengthText =
      "New name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be emptyNew name should not be 12345";
    cy.findByTestId("document-new-name").type(maxLengthText);
    cy.realPress(".");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-document-new-name-link").should(
      "have.text",
      "New name must be 252 characters or less"
    );
    cy.findByTestId("reclassify-document-new-name-link").click();
    cy.focused().should("have.id", "document-new-name");
    cy.get("#document-new-name-error").should(
      "have.text",
      "Error: New name must be 252 characters or less"
    );
    cy.findByTestId("document-new-name").clear().type("abc");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("div-reclassify")
      .contains("button", "Back", { timeout: 10000 })
      .should("be.visible")
      .click();
    cy.findByTestId("div-reclassify")
      .contains("button", "Back", { timeout: 10000 })
      .should("be.visible")
      .click();

    //Statement type validation
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");
    cy.findByTestId("reclassify-document-type").select("MG11");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the statement details");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 3);
    cy.findByTestId("reclassify-statement-witness-link").should(
      "have.text",
      "Statement witness should not be empty"
    );
    cy.findByTestId("reclassify-statement-date-link").should(
      "have.text",
      "Statement date must be a real date"
    );
    cy.findByTestId("reclassify-statement-number-link").should(
      "have.text",
      "Statement number should not be empty"
    );
    cy.findByTestId("reclassify-statement-witness-link").click();
    cy.focused().should("have.id", "statement-witness");
    cy.get("#statement-witness-error").should(
      "have.text",
      "Error: Statement witness should not be empty"
    );
    cy.findByTestId("reclassify-statement-number-link").click();
    cy.focused().should("have.id", "statement-number");
    cy.get("#statement-number-error").should(
      "have.text",
      "Error: Statement number should not be empty"
    );
    cy.findByTestId("reclassify-statement-date-link").click();
    cy.focused().should("have.id", "statement-day");
    cy.get("#statement-date-error").should(
      "have.text",
      "Error: Statement date must be a real date"
    );
    cy.findByTestId("reclassify-statement-witness").select("PC Blaynee_S");
    cy.findByTestId("reclassify-statement-number").type("3");
    cy.get('input[type="text"][name="statement-date-day"]').type("19");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 2);
    cy.findByTestId("reclassify-statement-date-link").should(
      "have.text",
      "Statement date must be a real date"
    );
    cy.findByTestId("reclassify-statement-number-link").should(
      "have.text",
      "Statement number 3 already exist"
    );
    cy.get("#statement-date-error").should(
      "have.text",
      "Error: Statement date must be a real date"
    );
    cy.findByTestId("reclassify-statement-date-link").click();
    cy.focused().should("have.id", "statement-month");
    cy.get('input[type="text"][name="statement-date-month"]').type("10");
    cy.findByTestId("reclassify-statement-number-link").click();
    cy.focused().should("have.id", "statement-number");
    cy.get("#statement-number-error").should(
      "have.text",
      "Error: Statement number 3 already exist"
    );
    cy.findByTestId("reclassify-statement-number").type("5");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-statement-date-link").should(
      "have.text",
      "Statement date must be a real date"
    );
    cy.get("#statement-date-error").should(
      "have.text",
      "Error: Statement date must be a real date"
    );
    cy.findByTestId("reclassify-statement-date-link").click();
    cy.focused().should("have.id", "statement-year");
    cy.get('input[type="text"][name="statement-date-year"]').type("1980");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("div-reclassify")
      .contains("button", "Back", { timeout: 10000 })
      .should("be.visible")
      .click();
    cy.findByTestId("reclassify-cancel-btn").click();
    cy.findByTestId("div-reclassify").should("not.exist");

    //Exhibit validation
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");
    cy.findByTestId("reclassify-document-type").select("MG15(SDN)");
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the exhibit details");

    cy.findByTestId("exhibit-item-name").clear();
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 2);
    cy.findByTestId("reclassify-exhibit-item-name-link").should(
      "have.text",
      "Exhibit item name should not be empty"
    );
    cy.findByTestId("reclassify-exhibit-reference-link").should(
      "have.text",
      "Exhibit reference should not be empty"
    );
    cy.findByTestId("reclassify-exhibit-reference-link").click();
    cy.focused().should("have.id", "exhibit-reference");
    cy.get("#exhibit-reference-error").should(
      "have.text",
      "Error: Exhibit reference should not be empty"
    );
    cy.findByTestId("exhibit-reference").type("test_ref");
    cy.findByTestId("reclassify-exhibit-item-name-link").click();
    cy.focused().should("have.id", "exhibit-item-name");
    cy.get("#exhibit-item-name-error").should(
      "have.text",
      "Error: Exhibit item name should not be empty"
    );
    cy.findByTestId("exhibit-item-name").type("tes{t>");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-exhibit-item-name-link").should(
      "have.text",
      "Exhibit item name should not contain invalid characters {>"
    );
    cy.findByTestId("reclassify-exhibit-item-name-link").click();
    cy.focused().should("have.id", "exhibit-item-name");
    cy.get("#exhibit-item-name-error").should(
      "have.text",
      "Error: Exhibit item name should not contain invalid characters {>"
    );
    cy.findByTestId("exhibit-item-name").clear().type(maxLengthText);
    cy.realPress(".");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-exhibit-item-name-link").should(
      "have.text",
      "Exhibit item name must be 252 characters or less"
    );
    cy.findByTestId("reclassify-exhibit-item-name-link").click();
    cy.focused().should("have.id", "exhibit-item-name");
    cy.get("#exhibit-item-name-error").should(
      "have.text",
      "Error: Exhibit item name must be 252 characters or less"
    );
    cy.findByTestId("exhibit-item-name").clear().type("abc");
    cy.findByTestId("exhibit-select-producer").select(
      "Other producer or witness"
    );
    cy.findByTestId("reclassify-continue-btn").click();

    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-exhibit-other-producer-name-link").should(
      "have.text",
      "Exhibit new producer or witness should not be empty"
    );
    cy.findByTestId("reclassify-exhibit-other-producer-name-link").click();
    cy.focused().should("have.id", "exhibit-other-producer-name");
    cy.get("#exhibit-other-producer-name-error").should(
      "have.text",
      "Error: Exhibit new producer or witness should not be empty"
    );
    cy.findByTestId("exhibit-other-producer-name").type("producer=@1");
    cy.get(
      'input[type="radio"][name="radio-document-used-status"][value="NO"]'
    ).check();
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("reclassify-error-summary")
      .find("li")
      .should("have.length", 1);
    cy.findByTestId("reclassify-exhibit-other-producer-name-link").should(
      "have.text",
      "Exhibit new producer or witness text should not contain invalid characters =@1"
    );
    cy.findByTestId("reclassify-exhibit-other-producer-name-link").click();
    cy.focused().should("have.id", "exhibit-other-producer-name");
    cy.get("#exhibit-other-producer-name-error").should(
      "have.text",
      "Error: Exhibit new producer or witness text should not contain invalid characters =@1"
    );
    cy.findByTestId("exhibit-other-producer-name").clear().type("producer");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("reclassify-save-btn").click();
    // all buttons should be disabled when saving reclassification
    cy.findByTestId("div-reclassify")
      .contains("button", "Back", { timeout: 10000 })
      .should("be.visible")
      .should("be.disabled");

    for (let i = 0; i < 3; i++) {
      cy.findByTestId("reclassify-summary")
        .find("tbody tr")
        .eq(i)
        .contains("button", "Change")
        .should("be.disabled");
    }
    cy.waitUntil(() => {
      return saveReclassifyRequestObject.body;
    }).then(() => {
      expect(saveReclassifyRequestObject.body).to.deep.equal(
        JSON.stringify(expectedSaveReclassifyPayload)
      );
    });
  });

  it("should be able to navigate to stage 2 from stage 3 and change any details", () => {
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");
    cy.findByTestId("reclassify-document-type").select("MG15(SDN)");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the exhibit details");

    cy.findByTestId("exhibit-reference").type("test_ref");
    cy.findByTestId("exhibit-select-producer").select("PC Blaynee");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(4)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Status");
        expect(cells.eq(1)).to.have.text("Used");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(4)
      .contains("button", "Change")
      .click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the exhibit details");
    cy.findByTestId("exhibit-reference").clear().type("test_ref_changed");
    cy.findByTestId("exhibit-select-producer").select("PC Lucy");
    cy.get(
      'input[type="radio"][name="radio-document-used-status"][value="NO"]'
    ).check();
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Check your answers");
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(0)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Type");
        expect(cells.eq(1)).to.have.text("MG15(SDN)");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(1)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Item Name");
        expect(cells.eq(1)).to.have.text("MCLOVEMG3");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(2)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Exhibit Reference");
        expect(cells.eq(1)).to.have.text("test_ref_changed");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(3)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Exhibit Producer");
        expect(cells.eq(1)).to.have.text("PC Lucy");
        expect(cells.eq(2)).to.have.text("Change");
      });
    cy.findByTestId("reclassify-summary")
      .find("tbody tr")
      .eq(4)
      .find("td")
      .then((cells) => {
        expect(cells.eq(0)).to.have.text("Status");
        expect(cells.eq(1)).to.have.text("Unused");
        expect(cells.eq(2)).to.have.text("Change");
      });
  });

  it("should show error if it failed to retrieve materialList", () => {
    cy.overrideRoute(
      MATERIAL_TYPE_LIST,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "get"
    );
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.get("h1").should(
      "have.text",
      "Sorry, there is a problem with the service"
    );
    cy.get("body").contains("Error: Failed to retrieve material type list");
  });

  it("should show error if it failed to retrieve exhibit producers", () => {
    cy.overrideRoute(
      EXHIBIT_PRODUCERS,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "get"
    );
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");
    cy.findByTestId("reclassify-document-type").select("MG15(SDN)");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.get("h1").should(
      "have.text",
      "Sorry, there is a problem with the service"
    );
    cy.get("body").contains("Error: Failed to retrieve exhibit producer data");
  });

  it("should show error if it failed to retrieve statement witness", () => {
    cy.overrideRoute(
      STATEMENT_WITNESS,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "get"
    );
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");
    cy.findByTestId("reclassify-document-type").select("MG11");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.get("h1").should(
      "have.text",
      "Sorry, there is a problem with the service"
    );
    cy.get("body").contains(
      "Error: Failed to retrieve statement witness details"
    );
  });

  it("should show error if it failed to retrieve statement witness numbers", () => {
    cy.overrideRoute(
      STATEMENT_WITNESS_NUMBERS,
      {
        type: "break",
        httpStatusCode: 500,
        timeMs: 500,
      },
      "get"
    );
    cy.visit("/case-details/12AB1111111/13401?reclassify=true");
    cy.findByTestId("btn-accordion-open-close-all").click();
    cy.findByTestId("document-housekeeping-actions-dropdown-1").click();
    cy.findByTestId("dropdown-panel").contains("Reclassify document").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "What type of document is this?");
    cy.findByTestId("reclassify-document-type").select("MG11");
    cy.findByTestId("reclassify-continue-btn").click();
    cy.findByTestId("div-reclassify")
      .find("h1")
      .should("have.length", 1)
      .and("have.text", "Enter the statement details");
    cy.findByTestId("reclassify-statement-witness").select("PC Blaynee_S");
    cy.get("h1").should(
      "have.text",
      "Sorry, there is a problem with the service"
    );
    cy.get("body").contains(
      "Error: Failed to retrieve statement witness numbers"
    );
  });
});
