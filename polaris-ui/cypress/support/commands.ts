// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add('login', (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add('drag', { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add('dismiss', { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite('visit', (originalFn, url, options) => { ... })
import "@testing-library/cypress/add-commands";
import { rest as mswRest } from "msw";

const apiPath = (path: string) =>
  new URL(path, Cypress.env("REACT_APP_GATEWAY_BASE_URL")).toString();

Cypress.Commands.add("overrideRoute", (apiRoute, response) => {
  return cy.window().then((/*window*/) => {
    // the `window` object passed into the function does not have `msw`
    //  attached to it - so god knows what's happening.  The ambient
    //  `window` does have msw, so just use that.
    const msw = (window as any).msw;

    msw.worker.use(
      (msw.rest as typeof mswRest).get(apiPath(apiRoute), (req, res, ctx) => {
        switch (response.type) {
          case "break":
            return res.once(ctx.status(response.httpStatusCode));
          case "delay":
            return res.once(ctx.delay(response.timeMs));
          default:
            return res.once(ctx.json(response.body));
        }
      })
    );
  });
});

Cypress.Commands.add(
  "selectPDFTextElement",
  (matchString: string, targetCount = 0) => {
    cy.get(`.textLayer span:contains(${matchString})`)
      .last()
      .each((element, index) => {
        if (index === targetCount) {
          cy.wrap(element)
            .trigger("mousedown")
            .then(() => {
              const el = element[0];
              const document = el.ownerDocument;
              const range = document.createRange();
              range.selectNodeContents(el);
              document.getSelection()?.removeAllRanges();
              document.getSelection()?.addRange(range);
            })
            .trigger("mouseup");
          cy.document().trigger("selectionchange");
          return false;
        }
      });
  }
);

export {};
