describe("Search PII", () => {
  describe("Feature flag 'ON'", () => {
    it("Should show turn on/off redaction suggestions menu correctly and Should not call the pii request if the polarisdocumentversionId is not changed", () => {
      const piiRequestCounter = { count: 0 };
      cy.trackRequestCount(
        piiRequestCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/documents/12/pii"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions")
        .should("not.exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions")
        .should("exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .should("not.exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions")
        .click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .should("exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions")
        .should("not.exist");
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .should("exist");
      cy.window().then(() => {
        expect(piiRequestCounter.count).to.equal(1);
      });
    });

    it("Should correctly show all the redaction suggestions and redaction suggestion header", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .click();
      cy.findByTestId("search-pii-header").should("exist");
      cy.findByTestId("search-pii-header").contains("Potential redactions");
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 23 redactions"
      );
    });

    it("Should turn off the redaction suggestions on clicking Remove all redactions", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .click();

      cy.findByTestId("search-pii-header").should("exist");
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 23 redactions"
      );
      cy.findByTestId("btn-link-removeAll-0")
        .contains("Remove all redactions")
        .click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .should("exist");
    });

    it("Should show suggested redaction warning message", () => {
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .click();

      cy.findByTestId("search-pii-header").should("exist");
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 23 redactions"
      );
      cy.findByTestId("btn-save-redaction-0").click();

      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("div-modal").contains("h2", "Use potential redactions?");
      cy.findByTestId("div-modal").contains(
        "Your remaining 23 potential redactions will also be redacted, if you choose to continue"
      );
      cy.findByTestId("btn-cancel").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("btn-modal-close").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").contains("h2", "Use potential redactions?");
      cy.findByTestId("btn-continue").click();
      cy.findByTestId("warning-error-summary").should("exist");
      cy.findByTestId("terms-and-condition-link").should("exist");
      cy.findByTestId("terms-and-condition-link").should(
        "have.text",
        "Please accept you have manually checked all selected redactions in the document"
      );
      cy.get("#terms-and-condition-error").should(
        "have.text",
        "Error: Please accept you have manually checked all selected redactions in the document"
      );
      cy.findByTestId("terms-and-condition-link").click();
      cy.focused().should("have.id", "terms-and-condition");
      cy.get("#terms-and-condition-error").should(
        "have.text",
        "Error: Please accept you have manually checked all selected redactions in the document"
      );
      cy.findByTestId("terms-and-condition").click();
      cy.findByTestId("btn-continue").click();
      cy.findByTestId("div-modal")
        .contains("h2", "Use potential redactions?")
        .should("not.exist");
      cy.findByTestId("div-modal")
        .contains("99ZZ9999999 - Redaction Log")
        .should("exist");
      cy.findByTestId("div-modal").contains("li", "8 - Named individuals");
      cy.findByTestId("div-modal").contains("li", "5 - Phone number");
      cy.findByTestId("div-modal").contains("li", "4 - Others");
      cy.findByTestId("div-modal").contains("li", "2 - Addresses");
      cy.findByTestId("div-modal").contains("li", "2 - NI Numbers");
      cy.findByTestId("div-modal").contains("li", "1 - Email address");
      cy.findByTestId("div-modal").contains("li", "1 - Location");
    });

    it("Should have the correct redaction save request", () => {
      const expectedSaveRedactionPayload = {
        documentId: "12",
        redactions: [
          {
            pageIndex: 1,
            height: 11.68,
            width: 8.26,
            redactionCoordinates: [
              { x1: 1.94, y1: 8.34, x2: 3.09, y2: 8.13 },
              { x1: 3.16, y1: 5.95, x2: 4.3, y2: 5.74 },
              { x1: 4.24, y1: 5.51, x2: 6.5, y2: 5.3 },
              { x1: 6.57, y1: 5.28, x2: 7.24, y2: 5.07 },
              { x1: 0.98, y1: 5.06, x2: 3.99, y2: 4.86 },
              { x1: 2, y1: 5.28, x2: 2.96, y2: 5.11 },
              { x1: 6.28, y1: 4.84, x2: 7.28, y2: 4.66 },
              { x1: 1.15, y1: 4.61, x2: 2.2, y2: 4.44 },
              { x1: 6.48, y1: 4, x2: 7.08, y2: 3.83 },
              { x1: 0.97, y1: 3.78, x2: 1.53, y2: 3.57 },
              { x1: 5.63, y1: 3.56, x2: 6.64, y2: 3.38 },
              { x1: 6.62, y1: 3.11, x2: 7.23, y2: 2.94 },
              { x1: 0.97, y1: 2.89, x2: 1.53, y2: 2.68 },
              { x1: 2.48, y1: 2.44, x2: 3.49, y2: 2.27 },
              { x1: 1.9, y1: 2.05, x2: 3.04, y2: 1.84 },
              { x1: 0.97, y1: 1.83, x2: 1.94, y2: 1.65 },
              { x1: 4.21, y1: 1.83, x2: 5.26, y2: 1.65 },
            ],
          },
          {
            pageIndex: 2,
            height: 11.68,
            width: 8.26,
            redactionCoordinates: [
              { x1: 1.52, y1: 10.68, x2: 3.02, y2: 10.5 },
              { x1: 3.2, y1: 10.68, x2: 4.57, y2: 10.5 },
              { x1: 2.15, y1: 10.46, x2: 3.29, y2: 10.25 },
              { x1: 0.97, y1: 10.46, x2: 1.88, y2: 10.25 },
              { x1: 3.27, y1: 9.44, x2: 4.95, y2: 9.27 },
              { x1: 5.19, y1: 9.44, x2: 6.2, y2: 9.27 },
              { x1: 0.99, y1: 9.44, x2: 1.77, y2: 9.27 },
              { x1: 2.07, y1: 9.05, x2: 3.25, y2: 8.84 },
              { x1: 6.29, y1: 9.05, x2: 6.96, y2: 8.84 },
              { x1: 0.98, y1: 8.83, x2: 3.99, y2: 8.63 },
            ],
          },
        ],
        pii: {
          categories: [
            {
              polarisCategory: "Named individual",
              providerCategory: "Person",
              countSuggestions: 8,
              countAccepted: 8,
              countAmended: 0,
            },
            {
              polarisCategory: "Email address",
              providerCategory: "Email",
              countSuggestions: 1,
              countAccepted: 1,
              countAmended: 0,
            },
            {
              polarisCategory: "Address",
              providerCategory: "Address",
              countSuggestions: 2,
              countAccepted: 2,
              countAmended: 0,
            },
            {
              polarisCategory: "Location",
              providerCategory: "IPAddress",
              countSuggestions: 1,
              countAccepted: 1,
              countAmended: 0,
            },
            {
              polarisCategory: "Phone number",
              providerCategory: "PhoneNumber",
              countSuggestions: 5,
              countAccepted: 5,
              countAmended: 0,
            },
            {
              polarisCategory: "NI Number",
              providerCategory: "UKNationalInsuranceNumber",
              countSuggestions: 2,
              countAccepted: 2,
              countAmended: 0,
            },
            {
              polarisCategory: "Other",
              providerCategory: "CreditCardNumber",
              countSuggestions: 2,
              countAccepted: 2,
              countAmended: 0,
            },
            {
              polarisCategory: "Other",
              providerCategory: "EUDriversLicenseNumber",
              countSuggestions: 1,
              countAccepted: 1,
              countAmended: 0,
            },
            {
              polarisCategory: "Other",
              providerCategory: "EUPassportNumber",
              countSuggestions: 1,
              countAccepted: 1,
              countAmended: 0,
            },
          ],
        },
      };

      const saveRequestObject = { body: "" };
      cy.trackRequestBody(
        saveRequestObject,
        "PUT",
        "/api/urns/12AB1111111/cases/13401/documents/12"
      );
      cy.visit("/case-details/12AB1111111/13401");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .click();

      cy.findByTestId("search-pii-header").should("exist");
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 23 redactions"
      );
      cy.findByTestId("btn-save-redaction-0").click();

      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("div-modal").contains("h2", "Use potential redactions?");
      cy.findByTestId("div-modal").contains(
        "Your remaining 23 potential redactions will also be redacted, if you choose to continue"
      );
      cy.findByTestId("btn-continue").click();
      cy.get("#terms-and-condition-error").should(
        "have.text",
        "Error: Please accept you have manually checked all selected redactions in the document"
      );
      cy.findByTestId("terms-and-condition").click();
      cy.findByTestId("btn-continue").click();
      cy.findByTestId("div-modal")
        .contains("h2", "Use potential redactions?")
        .should("not.exist");
      cy.findByTestId("div-modal")
        .contains("99ZZ9999999 - Redaction Log")
        .should("exist");
      cy.findByTestId("div-modal").contains("li", "8 - Named individuals");
      cy.findByTestId("div-modal").contains("li", "5 - Phone numbers");
      cy.findByTestId("div-modal").contains("li", "4 - Others");
      cy.findByTestId("div-modal").contains("li", "2 - Addresses");
      cy.findByTestId("div-modal").contains("li", "2 - NI Numbers");
      cy.findByTestId("div-modal").contains("li", "1 - Email address");
      cy.findByTestId("div-modal").contains("li", "1 - Location");

      //assertion on the redaction save request
      cy.waitUntil(() => {
        return saveRequestObject.body;
      }).then(() => {
        expect(saveRequestObject.body).to.deep.equal(
          JSON.stringify(expectedSaveRedactionPayload)
        );
      });
    });
  });

  describe("Feature Flag Off", () => {
    it("Should not show, turn on redaction suggestions menu item if the feature flag is off", () => {
      cy.visit("/case-details/12AB1111111/13401?searchPII=false");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions")
        .should("not.exist");
    });
  });
});
