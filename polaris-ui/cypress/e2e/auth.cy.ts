// import { CASE_SEARCH_ROUTE } from "../../src/mock-api/routes";

// describe("cookie authentication", () => {
//   it("can (re)auth when an api-call fails with a 403 status", () => {
//     Cypress.on("uncaught:exception", (err, runnable) => {
//       // returning false here prevents Cypress from
//       // failing the test
//       return false;
//     });
//     cy.overrideRoute(CASE_SEARCH_ROUTE, {
//       type: "break",
//       httpStatusCode: 403,
//     });
//     cy.visit("/case-search-results?urn=12AB1111111", {
//       failOnStatusCode: false,
//     });
//     cy.visit("/case-search-results?urn=12AB1111111", {
//       failOnStatusCode: false,
//     });
//   });
// });

// export {};
