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
import "cypress-real-events";
import { setupMockApi } from "../../src/mock-api/browser";
import "cypress-axe";
// Alternatively you can use CommonJS syntax:
// require('./commands')

// Prevent React error boundaries bubbling to Cypress as "uncaught" test failures.
// See https://github.com/cypress-io/cypress/issues/7196
Cypress.on("uncaught:exception", () => {
  return false;
});

// ---------- Diagnostics (optional but helpful) ----------
console.log("polaris-ui baseUrl:", Cypress.config("baseUrl"));
console.log(
  "REACT_APP_GATEWAY_BASE_URL:",
  Cypress.env("REACT_APP_GATEWAY_BASE_URL")
);
console.log(
  "REACT_APP_MOCK_API_SOURCE:",
  Cypress.env("REACT_APP_MOCK_API_SOURCE")
);
console.log(
  "REACT_APP_MOCK_API_MAX_DELAY:",
  Cypress.env("REACT_APP_MOCK_API_MAX_DELAY")
);
console.log(
  "REACT_APP_REDACTION_LOG_BASE_URL:",
  Cypress.env("REACT_APP_REDACTION_LOG_BASE_URL")
);
// -------------------------------------------------------

// Helper to validate URL-ish strings
const isValidUrl = (u: unknown): u is string =>
  typeof u === "string" && /^https?:\/\//i.test(u);

// Allow disabling mock API setup for quick sanity runs
const enableMockApi = Cypress.env("ENABLE_MOCK_API") !== false;

/**
 * Global hook: before each test run, setup the mock API (if enabled).
 * We guard against invalid/missing URLs to avoid "Failed to construct 'URL': Invalid URL".
 */
Cypress.on("test:before:run:async", async () => {
  if (!enableMockApi) {
    console.info("[mock-api] Skipped (ENABLE_MOCK_API=false).");
    return;
  }

  const baseUrl = Cypress.env("REACT_APP_GATEWAY_BASE_URL");
  const redactionLogUrl = Cypress.env("REACT_APP_REDACTION_LOG_BASE_URL");

  // If gateway base URL is invalid, skip setup to avoid throwing in URL constructors.
  if (!isValidUrl(baseUrl)) {
    console.warn(
      "[mock-api] Invalid REACT_APP_GATEWAY_BASE_URL; skipping mock-api setup.",
      {
        baseUrl,
      }
    );
    return;
  }

  // redactionLogUrl is required by MockApiConfig as a string.
  // If it's missing/invalid, default to the empty string (Option A).
  const safeRedactionLogUrl = isValidUrl(redactionLogUrl)
    ? redactionLogUrl
    : "";

  await setupMockApi({
    sourceName: Cypress.env("REACT_APP_MOCK_API_SOURCE"),
    baseUrl, // valid by guard above
    maxDelayMs: Cypress.env("REACT_APP_MOCK_API_MAX_DELAY"),
    publicUrl: "",
    redactionLogUrl: safeRedactionLogUrl,
  });
});

// ---------- Extend Cypress types ----------
declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace Cypress {
    interface Chainable {
      /**
       * Custom command to select first span element matching the given string
       */
      selectPDFTextElement(matchString: string): void;

      getElementAsync(element: string): any;

      overrideRoute(
        apiRoute: string,
        response:
          | {
              type: "break";
              httpStatusCode: number;
              timeMs?: number;
              body?: any;
            }
          | { type: "delay"; timeMs: number }
          | { type?: false; timeMs?: number; body: any },
        method?: "get" | "post" | "put",
        baseUrl?: string
      ): Chainable<AUTWindow>;

      trackRequestCount(
        counter: { count: number },
        method: "POST" | "GET" | "PUT" | "DELETE",
        pathname?: string | RegExp
      ): void;

      trackRequestBody(
        requestObject: { body: string },
        method: "POST" | "PUT",
        pathname: string
      ): void;
    }
  }
}
