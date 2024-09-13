/// <reference types="cypress" />

const { TARGET_URN, TARGET_CASE_ID } = Cypress.env()
describe("Reauth and handover flows", () => {
  describe(
    "Reauth flow",
    // Note: this is not run in prod because the e2e test agent cannot see the cms.cps.gov.uk/polaris
    //  handover endpoint.  It will be possible to rejig the config/logic of the reauth flow to
    //  allow the e2e tests to run in prod by e.g. overriding the reauth url config value.
    { tags: ["@ci", "@notrunprodci", "@ci-chunk-2"] },
    () => {
      it("can send the user around the reauth flow", () => {
        cy.on("uncaught:exception", () => false)

        // have we logged in OK?
        cy.loginToAD()
          .visit(`/polaris-ui/case-search-results?urn=${TARGET_URN}`)
          // we expect to not be logged-in to CMS...
          .contains("CMS_AUTH_ERROR")
          // ... and now we do log in
          .loginToCms()

        cy.visit(`/polaris-ui/case-search-results?urn=${TARGET_URN}`)

        // if we find the target URN we have made a successful round trip to DDEI
        cy.findByTestId(`link-${TARGET_URN}`)
      })
    }
  )

  describe("Inbound handover", { tags: ["@ci", "@ci-chunk-2"] }, () => {
    it("can receive a well formed triage handover and redirect the user to the requested case", () => {
      cy.on("uncaught:exception", () => false)

      const contextObject = {
        caseId: +TARGET_CASE_ID,
        taskId: 1,
        taskTypeId: 2,
      }

      const contextQueryParam = `${encodeURIComponent(
        JSON.stringify(contextObject)
      )}`

      cy.fullLogin().visit(`/polaris-ui/go?ctx=${contextQueryParam}`)

      // temporary logic to assert that context is handed-down to the page
      cy.contains('{"contextType":"triage","taskId":1,"taskTypeId":2}')
    })

    it("can receive a well formed triage handover when the user is not logged in to CMS and send the user around the reauth flow", () => {
      cy.on("uncaught:exception", () => false)

      const contextObject = {
        caseId: +TARGET_CASE_ID,
        taskId: 1,
        taskTypeId: 2,
      }

      const contextQueryParam = `${encodeURIComponent(
        JSON.stringify(contextObject)
      )}`

      cy.loginToAD().visit(`/polaris-ui/go?ctx=${contextQueryParam}`)

      cy.url().should("contain", "auth-refresh")
    })
  })
})
