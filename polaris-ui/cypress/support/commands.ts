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
import "cypress-wait-until";

const apiPath = (path: string, baseUrl: string) =>
  new URL(path, baseUrl).toString();

Cypress.Commands.add(
  "overrideRoute",
  (
    apiRoute,
    response,
    method = "get",
    baseUrl = Cypress.env("REACT_APP_GATEWAY_BASE_URL")
  ) => {
    return cy.window().then((/*window*/) => {
      // the `window` object passed into the function does not have `msw`
      //  attached to it - so god knows what's happening.  The ambient
      //  `window` does have msw, so just use that.
      const msw = (window as any).msw;

      msw.worker.use(
        (msw.rest as typeof mswRest)[`${method}`](
          apiPath(apiRoute, baseUrl),
          (req, res, ctx) => {
            switch (response.type) {
              case "break":
                return res.once(
                  ctx.delay(response.timeMs || 0),
                  ctx.status(response.httpStatusCode),
                  ctx.body(response.body)
                );
              case "delay":
                return res.once(ctx.delay(response.timeMs));
              default:
                return res.once(ctx.json(response.body));
            }
          }
        )
      );
    });
  }
);
Cypress.Commands.add("selectPDFTextElement", (matchString: string) => {
  cy.wait(100);
  cy.get(`.textLayer span:contains(${matchString})`)
    .filter(":not(:has(*))")
    .first()
    .then((element) => {
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
    });
});

Cypress.Commands.add(
  "trackRequestCount",
  (requestCounter, method, pathname) => {
    cy.window().then((win) => {
      const worker = (window as any).msw.worker;
      worker.events.on("request:start", (req: any) => {
        if (!pathname && req.method === method) {
          requestCounter.count++;
          return;
        }
        if (req.method === method && req.url.pathname === pathname) {
          requestCounter.count++;
        }
      });
    });
  }
);

Cypress.Commands.add("trackRequestBody", (requestObject, method, pathname) => {
  cy.window().then((win) => {
    const worker = (window as any).msw.worker;
    worker.events.on("request:start", (req: any) => {
      if (req.method === method && req.url.pathname === pathname) {
        requestObject.body = req.body;
      }
    });
  });
});

export {};
