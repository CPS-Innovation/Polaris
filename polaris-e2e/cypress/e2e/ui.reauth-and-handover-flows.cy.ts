/// <reference types="cypress" />

const { TARGET_URN, TARGET_CASE_ID } = Cypress.env();
describe("Reauth and handover flows", { tags: ["@ci", "@ci-chunk-2"] }, () => {
  describe("Reauth flow", () => {
    xit("can obtain cms auth", () => {
      cy.on("uncaught:exception", () => false);

      // have we logged in OK?
      cy.loginToAD()
        .visit(`/polaris-ui/case-search-results?urn=${TARGET_URN}`)
        // we expect to not be logged-in to CMS...
        .contains("CMS_AUTH_ERROR");

      // ... and now we do log in
      cy.loginToCms();

      cy.visit(`/polaris-ui/case-search-results?urn=${TARGET_URN}`);

      // if we find the target URN we have made a successful round trip to DDEI
      cy.findByTestId(`link-${TARGET_URN}`);
    });
  });

  describe("Inbound handover", () => {
    xit("can receive a well formed triage handover and redirect the user to the requested case", () => {
      cy.on("uncaught:exception", () => false);

      const contextObject = {
        caseId: +TARGET_CASE_ID,
        taskId: 1,
        taskTypeId: 2,
      };

      const contextQueryParam = `${encodeURIComponent(
        JSON.stringify(contextObject)
      )}`;

      cy.fullLogin().visit(`/polaris-ui/go?ctx=${contextQueryParam}`);

      cy.findByTestId("txt-case-urn").contains(TARGET_URN);
      // temporary logic to assert that context is handed-down to the page
      cy.contains('{"contextType":"triage","taskId":1,"taskTypeId":2}');
    });

    xit("can receive a well formed triage handover when the user is not logged in to CMS and send the user around the reauth flow", () => {
      cy.on("uncaught:exception", () => false);

      const contextObject = {
        caseId: +TARGET_CASE_ID,
        taskId: 1,
        taskTypeId: 2,
      };

      const contextQueryParam = `${encodeURIComponent(
        JSON.stringify(contextObject)
      )}`;

      cy.loginToAD()
        .visit(`/polaris-ui/go?ctx=${contextQueryParam}`) // we expect to not be logged-in to CMS...
        .contains("CMS_AUTH_ERROR")
        // ... and now we do log in
        .loginToCms();

      cy.visit(`/polaris-ui/go?ctx=${contextQueryParam}`);
      cy.findByTestId("txt-case-urn").contains(TARGET_URN);
    });
  });
});
