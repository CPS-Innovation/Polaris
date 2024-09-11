/// <reference types="cypress" />

const { TARGET_CASE_ID } = Cypress.env()

describe("Inbound handover", { tags: ["@ci", "@ci-chunk-4"] }, () => {
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
})

export {}
