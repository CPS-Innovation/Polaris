import cypressDetailsDataSource from "../../src/mock-api/data/caseDetails.cypress";
import { CASE_ROUTE } from "../../src/mock-api/routes";

describe("State Retention", () => {
  const caseDetailsResult = {
    ...cypressDetailsDataSource(13401),
    uniqueReferenceNumber: "12AB1111111",
  };

  it("State should be retained if we are refreshing casework app when the state retention feature flag is true", () => {
    const documentListCounter = { count: 0 };
    cy.trackRequestCount(
      documentListCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/documents"
    );

    const caseDetailsCounter = { count: 0 };
    cy.trackRequestCount(
      caseDetailsCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401"
    );
    cy.overrideRoute(CASE_ROUTE, {
      body: caseDetailsResult,
      timeMs: 1000,
    });
    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("txt-please-wait-page-heading").should("exist");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Open all folders"
    );
    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId("link-document-2").click();
    cy.findByTestId("div-pdfviewer-0").should("exist").contains("CASE OUTLINE");
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-1")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.findByTestId("select-redaction-type").should("have.length", 1);
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click();

    cy.contains("There is 1 redaction");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Close all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("exist");
    cy.findByTestId("btn-save-redaction-1").should("exist");
    cy.findByTestId("tab-0").should("exist");
    cy.findByTestId("tab-1").should("exist");
    // this is to slow down the api response so that test will get time to verify the `txt-please-wait-page-heading`
    cy.overrideRoute(CASE_ROUTE, {
      body: caseDetailsResult,
      timeMs: 1000,
    });
    cy.reload();
    cy.wait(1000);

    cy.findByTestId("txt-please-wait-page-heading").should("not.exist");

    cy.contains("There is 1 redaction");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Close all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("exist");
    cy.findByTestId("btn-save-redaction-1").should("exist");
    cy.findByTestId("tab-0").should("exist");
    cy.findByTestId("tab-1").should("exist");
    //making sure the getCasedetails and the getDocumentlist apis are called even if the state is retained
    cy.window().then(() => {
      expect(documentListCounter.count).to.equal(2);
      expect(caseDetailsCounter.count).to.equal(2);
    });
  });
  it("State should not be retained if we are refreshing casework app when the state retention feature flag is false", () => {
    const documentListCounter = { count: 0 };
    cy.trackRequestCount(
      documentListCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/documents"
    );

    const caseDetailsCounter = { count: 0 };
    cy.trackRequestCount(
      caseDetailsCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401"
    );
    cy.overrideRoute(CASE_ROUTE, {
      body: caseDetailsResult,
      timeMs: 1000,
    });
    cy.visit("/case-details/12AB1111111/13401?stateRetention=false");
    cy.findByTestId("txt-please-wait-page-heading").should("exist");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Open all folders"
    );
    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId("link-document-2").click();
    cy.findByTestId("div-pdfviewer-0").should("exist").contains("CASE OUTLINE");
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-1")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.findByTestId("select-redaction-type").should("have.length", 1);
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click();

    cy.contains("There is 1 redaction");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Close all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("exist");
    cy.findByTestId("btn-save-redaction-1").should("exist");
    cy.findByTestId("tab-0").should("exist");
    cy.findByTestId("tab-1").should("exist");

    cy.overrideRoute(CASE_ROUTE, {
      body: caseDetailsResult,
      timeMs: 1000,
    });
    cy.reload();

    cy.findByTestId("txt-please-wait-page-heading").should("exist");
    cy.contains("There is 1 redaction").should("not.exist");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Open all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("not.exist");
    cy.findByTestId("btn-save-redaction-1").should("not.exist");
    cy.findByTestId("tab-0").should("not.exist");
    cy.findByTestId("tab-1").should("not.exist");
    cy.window().then(() => {
      expect(documentListCounter.count).to.equal(2);
      expect(caseDetailsCounter.count).to.equal(2);
    });
  });
  it("Should clear state retained for all the other caseIds, except the current one", () => {
    const documentListCounter = { count: 0 };
    cy.trackRequestCount(
      documentListCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/documents"
    );

    const caseDetailsCounter = { count: 0 };
    cy.trackRequestCount(
      caseDetailsCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401"
    );

    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Open all folders"
    );
    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId("link-document-2").click();
    cy.findByTestId("div-pdfviewer-0").should("exist").contains("CASE OUTLINE");
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-1")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.findByTestId("select-redaction-type").should("have.length", 1);
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click();

    cy.contains("There is 1 redaction");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Close all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("exist");
    cy.findByTestId("btn-save-redaction-1").should("exist");
    cy.findByTestId("tab-0").should("exist");
    cy.findByTestId("tab-1").should("exist");

    cy.reload();
    cy.wait(1000);
    
    //making sure state is retained
    cy.contains("There is 1 redaction");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Close all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("exist");
    cy.findByTestId("btn-save-redaction-1").should("exist");
    cy.findByTestId("tab-0").should("exist");
    cy.findByTestId("tab-1").should("exist");
    //making sure the getCasedetails and the getDocumentlist apis are called even if the state is retained
    cy.window().then(() => {
      expect(documentListCounter.count).to.equal(2);
      expect(caseDetailsCounter.count).to.equal(2);
    });
    //navigating to a new caseid
    cy.visit("/case-details/12AB1111111/123");

    //navigating back to the original caseid
    cy.visit("/case-details/12AB1111111/13401");
    cy.contains("There is 1 redaction").should("not.exist");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Open all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("not.exist");
    cy.findByTestId("btn-save-redaction-1").should("not.exist");
    cy.findByTestId("tab-0").should("not.exist");
    cy.findByTestId("tab-1").should("not.exist");
    cy.window().then(() => {
      expect(documentListCounter.count).to.equal(3);
      expect(caseDetailsCounter.count).to.equal(3);
    });
  });
  it("Should clear state, if the sessionStorage is cleared", () => {
    const documentListCounter = { count: 0 };
    cy.trackRequestCount(
      documentListCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401/documents"
    );

    const caseDetailsCounter = { count: 0 };
    cy.trackRequestCount(
      caseDetailsCounter,
      "GET",
      "/api/urns/12AB1111111/cases/13401"
    );

    cy.visit("/case-details/12AB1111111/13401");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Open all folders"
    );
    cy.findByTestId("btn-accordion-open-close-all").click();

    cy.findByTestId("link-document-2").click();
    cy.findByTestId("div-pdfviewer-0").should("exist").contains("CASE OUTLINE");
    cy.findByTestId("link-document-1").click();
    cy.findByTestId("div-pdfviewer-1")
      .should("exist")
      .contains("REPORT TO CROWN PROSECUTOR FOR CHARGING DECISION,");

    cy.selectPDFTextElement("WEST YORKSHIRE POLICE");
    cy.findByTestId("btn-redact").should("be.disabled");
    cy.findByTestId("select-redaction-type").should("have.length", 1);
    cy.findByTestId("select-redaction-type").select("2");
    cy.findByTestId("btn-redact").click();

    cy.contains("There is 1 redaction");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Close all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("exist");
    cy.findByTestId("btn-save-redaction-1").should("exist");
    cy.findByTestId("tab-0").should("exist");
    cy.findByTestId("tab-1").should("exist");
    cy.clearAllSessionStorage();

    cy.reload();
    cy.contains("There is 1 redaction").should("not.exist");
    cy.findByTestId("btn-accordion-open-close-all").should(
      "contain.text",
      "Open all folders"
    );
    cy.findByTestId("btn-link-removeAll-1").should("not.exist");
    cy.findByTestId("btn-save-redaction-1").should("not.exist");
    cy.findByTestId("tab-0").should("not.exist");
    cy.findByTestId("tab-1").should("not.exist");
    cy.window().then(() => {
      expect(documentListCounter.count).to.equal(2);
      expect(caseDetailsCounter.count).to.equal(2);
    });
  });
});
