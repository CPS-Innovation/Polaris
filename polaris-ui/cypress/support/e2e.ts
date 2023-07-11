// ***********************************************************
// This example support/index.js is processed and
// loaded automatically before your test files.
//
// This is a great place to put global configuration and
// behavior that modifies Cypress.
//
// You can change the location of this file or turn off
// automatically serving support files with the
// 'supportFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/configuration
// ***********************************************************

// Import commands.js using ES2015 syntax:
import "./commands";
import "@cypress/code-coverage/support";
import "cypress-hmr-restarter";
import "cypress-plugin-tab";
import { setupMockApi } from "../../src/mock-api/browser";
// Alternatively you can use CommonJS syntax:
// require('./commands')

Cypress.on("uncaught:exception", (err, runnable) => {
  // React error boundaries still attach errors to window.onerror
  //  so cypress thinks there is an uncontrolled error and tests fails
  //  see https://github.com/cypress-io/cypress/issues/7196
  return false;
});

Cypress.on("test:before:run:async", async () => {
  await setupMockApi({
    sourceName: Cypress.env().REACT_APP_MOCK_API_SOURCE,
    baseUrl: Cypress.env().REACT_APP_GATEWAY_BASE_URL,
    maxDelayMs: Cypress.env().REACT_APP_MOCK_API_MAX_DELAY,
    publicUrl: "",
  });
});

declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace Cypress {
    interface Chainable {
      /**
       * Custom command to select first span element matching the given string
       */
      selectPDFTextElement(matchString: string): void;
      overrideRoute(
        apiRoute: string,
        response:
          | { type: "break"; httpStatusCode: number; body?: any }
          | { type: "delay"; timeMs: number }
          | { type?: false; body: any },
        method?: "get" | "post" | "put"
      ): Chainable<AUTWindow>;
      trackRequestCount(
        counter: { count: number },
        method: "POST" | "GET" | "PUT",
        pathname?: string
      ): void;
    }
  }
}
