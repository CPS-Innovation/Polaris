describe("Search PII", () => {
  describe("Feature flag 'ON'", () => {
    it("Should show turn on/off redaction suggestions menu correctly and should not call the pii request if the polarisdocumentversionId is not changed", () => {
      const piiRequestCounter = { count: 0 };
      cy.trackRequestCount(
        piiRequestCounter,
        "GET",
        "/api/urns/12AB1111111/cases/13401/documents/12/pii"
      );
      cy.visit("/case-details/12AB1111111/13401?searchPII=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions2")
        .should("not.exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions 2")
        .should("exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .should("not.exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions 2")
        .click();
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .should("exist");
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions 2")
        .should("not.exist");
      //should turn off the redaction suggestions when the document is closed
      cy.findByTestId("tab-remove").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .should("exist");
      cy.window().then(() => {
        expect(piiRequestCounter.count).to.equal(1);
      });
    });

    it("Should show suggested redaction warning message if there are redaction suggestions accepted through acceptAll and redaction log correctly", () => {
      cy.visit("/case-details/12AB1111111/13401?searchPII=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .click();
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("div-highlight-dacaba11-312e-443a-b71e-9ca64f211bdf")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").should("exist");
      cy.findByTestId("redact-modal")
        .contains(`"William McCoy" appears 7 times in the document`)
        .should("exist");
      cy.findByTestId("redact-modal").findByTestId("btn-accept-all").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 7 redaction"
      );
      cy.findByTestId("search-pii-header").contains(
        "(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location,(1) Named individual"
      );
      cy.findByTestId("btn-save-redaction-0").click();

      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("div-modal").contains(
        "h2",
        "Confirm redaction suggestion"
      );
      cy.findByTestId("div-modal").contains(
        "You have chosen to 'accept all' for 7 redaction suggestions. If you choose to continue, redactions will be applied which you may not have reviewed individually"
      );
      cy.findByTestId("btn-cancel").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("btn-modal-close").click();
      cy.findByTestId("div-modal").should("not.exist");
      cy.findByTestId("btn-save-redaction-0").click();
      cy.findByTestId("div-modal").contains(
        "h2",
        "Confirm redaction suggestion"
      );
      cy.findByTestId("btn-continue").click();
      cy.findByTestId("warning-error-summary").should("exist");
      cy.findByTestId("terms-and-condition-link").should("exist");
      cy.findByTestId("terms-and-condition-link").should(
        "have.text",
        "Please confirm you have reviewed the whole document and the redactions to be applied are intended."
      );
      cy.get("#terms-and-condition-error").should(
        "have.text",
        "Error: Please confirm you have reviewed the whole document and the redactions to be applied are intended."
      );
      cy.findByTestId("terms-and-condition-link").click();
      cy.focused().should("have.id", "terms-and-condition");
      cy.findByTestId("terms-and-condition").click();
      cy.findByTestId("btn-continue").click();
      cy.findByTestId("div-modal")
        .contains("h2", "Confirm redaction suggestion")
        .should("not.exist");
      cy.findByTestId("div-modal")
        .contains("99ZZ9999999 - Redaction Log")
        .should("exist");
      cy.findByTestId("div-modal").contains("li", "7 - Named individual");
    });

    it("Should not show suggested redaction warning message if there are no redaction suggestions accepted through acceptAll", () => {
      cy.visit("/case-details/12AB1111111/13401?searchPII=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .click();
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("div-highlight-dacaba11-312e-443a-b71e-9ca64f211bdf")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").should("exist");
      cy.findByTestId("redact-modal")
        .contains(`"William McCoy" appears 7 times in the document`)
        .should("exist");
      cy.findByTestId("redact-modal").findByTestId("btn-accept").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There is 1 redaction"
      );
      cy.findByTestId("search-pii-header").contains(
        "(7) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("btn-save-redaction-0").click();

      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("div-modal")
        .contains("h2", "Confirm redaction suggestion")
        .should("not.exist");
      cy.findByTestId("div-modal")
        .contains("h2", "Confirm redaction suggestion")
        .should("not.exist");
      cy.findByTestId("div-modal")
        .contains("99ZZ9999999 - Redaction Log")
        .should("exist");
      cy.findByTestId("div-modal").contains("li", "1 - Named individual");
    });

    it("Should be able to perform accept, acceptAll,ignore,ignoreAll actions on the redaction suggestions", () => {
      cy.visit("/case-details/12AB1111111/13401?searchPII=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .click();

      cy.findByTestId("search-pii-header").should("exist");
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("div-highlight-dacaba11-312e-443a-b71e-9ca64f211bdf")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").should("exist");
      cy.findByTestId("redact-modal")
        .contains(`"William McCoy" appears 7 times in the document`)
        .should("exist");
      cy.findByTestId("redact-modal")
        .findByTestId("btn-accept")
        .should("exist");
      cy.findByTestId("redact-modal")
        .findByTestId("btn-accept-all")
        .should("exist");
      cy.findByTestId("redact-modal")
        .findByTestId("btn-ignore")
        .should("exist");
      cy.findByTestId("redact-modal")
        .findByTestId("btn-ignore-all")
        .should("exist");

      //btn ignore click
      cy.findByTestId("redact-modal").findByTestId("btn-ignore").click();
      cy.findByTestId("search-pii-header").contains(
        "(7) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions 2")
        .click();

      //btn ignore all click
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .click();
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("div-highlight-dacaba11-312e-443a-b71e-9ca64f211bdf")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").findByTestId("btn-ignore-all").click();
      cy.findByTestId("search-pii-header").contains(
        "(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location,(1) Named individual"
      );
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn off potential redactions 2")
        .click();
      //btn accept click
      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 2")
        .click();
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("div-highlight-dacaba11-312e-443a-b71e-9ca64f211bdf")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").findByTestId("btn-accept").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There is 1 redaction"
      );
      cy.findByTestId("search-pii-header").contains(
        "(7) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("btn-link-removeAll-0")
        .contains("Remove all redactions")
        .click();
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      //btn accept all click
      cy.findByTestId("div-highlight-dacaba11-312e-443a-b71e-9ca64f211bdf")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").findByTestId("btn-accept-all").click();
      cy.findByTestId("redaction-count-text-0").contains(
        "There are 7 redactions"
      );
      cy.findByTestId("search-pii-header").contains(
        "(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location,(1) Named individual"
      );
      cy.findByTestId("btn-link-removeAll-0")
        .contains("Remove all redactions")
        .click();
      cy.findByTestId("redaction-count-text-0").should("not.exist");
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
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
              { x1: 6.62, y1: 3.11, x2: 7.23, y2: 2.94 },
              { x1: 0.97, y1: 2.89, x2: 1.53, y2: 2.68 },
              { x1: 6.48, y1: 4, x2: 7.08, y2: 3.83 },
              { x1: 0.97, y1: 3.78, x2: 1.53, y2: 3.57 },
              { x1: 1.9, y1: 2.05, x2: 3.04, y2: 1.84 },
              { x1: 1.94, y1: 8.34, x2: 3.09, y2: 8.13 },
              { x1: 3.16, y1: 5.95, x2: 4.3, y2: 5.74 },
              { x1: 4.24, y1: 5.51, x2: 6.5, y2: 5.3 },
            ],
          },
          {
            pageIndex: 2,
            height: 11.68,
            width: 8.26,
            redactionCoordinates: [
              { x1: 2.07, y1: 9.05, x2: 3.25, y2: 8.84 },
              { x1: 2.15, y1: 10.46, x2: 3.29, y2: 10.25 },
            ],
          },
        ],
        documentModifications: [],
        pii: {
          categories: [
            {
              polarisCategory: "Address",
              providerCategory: "Address",
              countSuggestions: 2,
              countAccepted: 0,
              countAmended: 0,
            },
            {
              polarisCategory: "Location",
              providerCategory: "IPAddress",
              countSuggestions: 1,
              countAccepted: 0,
              countAmended: 0,
            },
            {
              polarisCategory: "Phone number",
              providerCategory: "PhoneNumber",
              countSuggestions: 5,
              countAccepted: 0,
              countAmended: 0,
            },
            {
              polarisCategory: "NI Number",
              providerCategory: "UKNationalInsuranceNumber",
              countSuggestions: 2,
              countAccepted: 0,
              countAmended: 0,
            },
            {
              polarisCategory: "Other",
              providerCategory: "CreditCardNumber",
              countSuggestions: 2,
              countAccepted: 0,
              countAmended: 0,
            },
            {
              polarisCategory: "Named individual",
              providerCategory: "Person",
              countSuggestions: 8,
              countAccepted: 7,
              countAmended: 0,
            },
            {
              polarisCategory: "Other",
              providerCategory: "EUDriversLicenseNumber",
              countSuggestions: 1,
              countAccepted: 0,
              countAmended: 0,
            },
            {
              polarisCategory: "Other",
              providerCategory: "EUPassportNumber",
              countSuggestions: 1,
              countAccepted: 0,
              countAmended: 0,
            },
            {
              polarisCategory: "Email address",
              providerCategory: "Email",
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
      cy.visit("/case-details/12AB1111111/13401?searchPII=true");
      cy.findByTestId("btn-accordion-open-close-all").click();
      cy.findByTestId("link-document-12").click();
      cy.findByTestId("div-pdfviewer-0")
        .contains("WITNESS STATEMENT")
        .should("exist");

      cy.findByTestId("document-actions-dropdown-0").click();
      cy.findByTestId("dropdown-panel")
        .contains("Turn on potential redactions 1")
        .click();

      cy.findByTestId("search-pii-header").should("exist");
      cy.findByTestId("search-pii-header").contains(
        "(8) Named individual,(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Email address,(1) Location"
      );
      cy.findByTestId("div-highlight-dacaba11-312e-443a-b71e-9ca64f211bdf")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").findByTestId("btn-accept-all").click();
      cy.findByTestId("div-highlight-fc62502f-c593-4d32-842e-1523416e49fe")
        .findByRole("button")
        .click();
      cy.findByTestId("redact-modal").findByTestId("btn-accept").click();

      cy.findByTestId("redaction-count-text-0").contains(
        "There are 8 redactions"
      );
      cy.findByTestId("search-pii-header").contains(
        "(5) Phone number,(4) Other,(2) Address,(2) NI Number,(1) Location,(1) Named individual"
      );

      cy.findByTestId("btn-save-redaction-0").click();

      cy.findByTestId("div-modal").should("exist");
      cy.findByTestId("div-modal").contains(
        "h2",
        "Confirm redaction suggestion"
      );
      cy.findByTestId("div-modal").contains(
        "You have chosen to 'accept all' for 7 redaction suggestions. If you choose to continue, redactions will be applied which you may not have reviewed individually"
      );
      cy.findByTestId("btn-continue").click();
      cy.get("#terms-and-condition-error").should(
        "have.text",
        "Error: Please confirm you have reviewed the whole document and the redactions to be applied are intended."
      );
      cy.findByTestId("terms-and-condition").click();
      cy.findByTestId("btn-continue").click();
      cy.findByTestId("div-modal")
        .contains("h2", "Confirm redaction suggestion")
        .should("not.exist");
      cy.findByTestId("div-modal")
        .contains("99ZZ9999999 - Redaction Log")
        .should("exist");
      cy.findByTestId("div-modal").contains("li", "7 - Named individuals");
      cy.findByTestId("div-modal").contains("li", "1 - Email address");

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
